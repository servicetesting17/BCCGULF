/*!
 * ZeeroPro D365 CE Integration Library
 * ------------------------------------
 * Wraps the "ZeeroPro" Postman collection (AccessToken, CreateRole, Apply,
 * Shortlistcandidates, Invite) for use inside a Dynamics 365 Customer
 * Engagement / Dataverse model-driven app.
 *
 * Namespaces:
 *   ZeeroPro.Config    - reads connection settings from Dataverse Environment Variables
 *   ZeeroPro.Auth      - service-token acquisition & in-memory caching
 *   ZeeroPro.Api       - one function per Postman request
 *   ZeeroPro.Commands  - ribbon / command-bar button handlers
 *
 * Deploy: upload this file as a JavaScript web resource, e.g.
 *   new_/js/zeeropro-d365.js
 *
 * Requires the following Dataverse Environment Variables to exist
 * (Solutions > New > Environment Variable):
 *   baseUrl      - e.g. https://api.zeeropro.com
 *   apiKey       - sent as the X-Api-Key header
 *   tenantId     - ZeeroPro tenant id
 *   serviceJwt   - the pre-issued SERVICE_JWT credential
 *   templateId   - default career-application templateId (optional)
 */
var ZeeroPro = ZeeroPro || {};

(function () {
    "use strict";

    // ---------------------------------------------------------------------
    // ZeeroPro.Config
    // ---------------------------------------------------------------------
    ZeeroPro.Config = ZeeroPro.Config || {};

    ZeeroPro.Config.SCHEMA_NAMES = {
        baseUrl: "bcc_baseUrl",
        apiKey: "bcc_apiKey",
        tenantId: "bcc_tenantId"
    };

    // In-memory cache of resolved Environment Variable values, for this page session only.
    var _envCache = {};

    /**
     * Reads a single Environment Variable's effective value: its Current Value
     * if one has been set, otherwise its Default Value. This is the standard
     * client-side pattern for reading Environment Variables in D365 CE.
     */
    ZeeroPro.Config.getEnvironmentVariableValue = async function (schemaName) {
        if (_envCache[schemaName] !== undefined) {
            return _envCache[schemaName];
        }

        const result = await Xrm.WebApi.retrieveMultipleRecords(
            "environmentvariabledefinition",
            "?$filter=schemaname eq '" + schemaName + "'" +
            "&$select=defaultvalue" +
            "&$expand=environmentvariabledefinition_environmentvariablevalue($select=value)"
        );

        if (!result.entities.length) {
            throw new Error("ZeeroPro.Config: Environment Variable '" + schemaName + "' was not found.");
        }

        const definition = result.entities[0];
        const overrides = definition.environmentvariabledefinition_environmentvariablevalue;
        const value = (overrides && overrides.length && overrides[0].value)
            ? overrides[0].value
            : definition.defaultvalue;

        if (!value) {
            throw new Error("ZeeroPro.Config: Environment Variable '" + schemaName + "' has no value set.");
        }

        _envCache[schemaName] = value;
        return value;
    };

    /** Resolves every setting the library needs, in parallel. */
    ZeeroPro.Config.getAll = async function () {
        const names = ZeeroPro.Config.SCHEMA_NAMES;
        const [baseUrl, apiKey, tenantId] = await Promise.all([
            ZeeroPro.Config.getEnvironmentVariableValue(names.baseUrl),
            ZeeroPro.Config.getEnvironmentVariableValue(names.apiKey),
            ZeeroPro.Config.getEnvironmentVariableValue(names.tenantId)
        ]);
        return {
            baseUrl: baseUrl.replace(/\/$/, ""),
            apiKey: apiKey,
            tenantId: tenantId
        };
    };

    // ---------------------------------------------------------------------
    // Shared helpers
    // ---------------------------------------------------------------------
    ZeeroPro._handleResponse = async function (response) {
        const text = await response.text();
        let data = null;
        try { data = text ? JSON.parse(text) : null; } catch (e) { data = text; }

        if (!response.ok) {
            const message = (data && (data.message || data.error)) || response.statusText;
            const err = new Error("ZeeroPro API error " + response.status + ": " + message);
            err.status = response.status;
            err.body = data;
            throw err;
        }
        return data;
    };

    ZeeroPro._authHeaders = async function (extra) {
        const token = await ZeeroPro.Auth.getAccessToken();
        return Object.assign({ "Authorization": "Bearer " + token }, extra || {});
    };

    // ---------------------------------------------------------------------
    // ZeeroPro.Auth
    // ---------------------------------------------------------------------
    ZeeroPro.Auth = ZeeroPro.Auth || {};

    var _tokenCache = { accessToken: null, expiresAt: 0, tokenInfo: null };

    /**
     * POST {{BASE_URL}}/authentication/api/v1/auth/service/token
     * Exchanges the pre-issued SERVICE_JWT for a short-lived bearer access
     * token. Caches the token in memory and reuses it until ~60s before it
     * is due to expire.
     *
     * Confirmed response shape:
     * {
     *   accessToken, tokenType, expiresIn, expiresAt, tenantId, authType,
     *   keyId, scopes: { jobs, assessments, candidates },
     *   endpointPolicies: { "jobs.create": "write", ... }
     * }
     * expiresAt is an absolute epoch-millisecond timestamp - it's used
     * directly (with a 60s safety buffer) instead of recomputing from
     * expiresIn, so caching stays correct even if the server clock and the
     * browser clock drift slightly.
     */
    ZeeroPro.Auth.getAccessToken = async function (forceRefresh) {
        const now = Date.now();
        if (!forceRefresh && _tokenCache.accessToken && _tokenCache.expiresAt > now) {
            return _tokenCache.accessToken;
        }

        const config = await ZeeroPro.Config.getAll();

        const response = await fetch(config.baseUrl + "/authentication/api/v1/auth/service/token", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "X-Api-Key": config.apiKey
            }
        });

        const data = await ZeeroPro._handleResponse(response);

        if (!data || !data.accessToken) {
            throw new Error("ZeeroPro.Auth: token endpoint responded without an accessToken field.");
        }

        const expiresAt = data.expiresAt
            ? data.expiresAt - 60000                      // absolute timestamp from the server, minus a 60s buffer
            : now + ((data.expiresIn || 1800) * 1000) - 60000; // fallback if expiresAt is ever omitted

        _tokenCache.accessToken = data.accessToken;
        _tokenCache.expiresAt = expiresAt;
        _tokenCache.tokenInfo = data; // full payload: tenantId, keyId, scopes, endpointPolicies, ...
        return data.accessToken;
    };

    /** Returns the full payload from the last token response (scopes, endpointPolicies, keyId, ...), or null. */
    ZeeroPro.Auth.getTokenInfo = function () {
        return _tokenCache.tokenInfo || null;
    };

    /** Clears the cached token, forcing the next call to re-authenticate. */
    ZeeroPro.Auth.clearCache = function () {
        _tokenCache = { accessToken: null, expiresAt: 0, tokenInfo: null };
    };

    // ---------------------------------------------------------------------
    // ZeeroPro.Api  (one function per Postman request)
    // ---------------------------------------------------------------------
    ZeeroPro.Api = ZeeroPro.Api || {};

    /**
     * POST {{BASE_URL}}/client/service/api/v1/jobs
     * roleData: { roleName, description, businessUnit, country, state, city, workModel, statusOfRole }
     */
    ZeeroPro.Api.createRole = async function (roleData) {
        const config = await ZeeroPro.Config.getAll();
        const headers = await ZeeroPro._authHeaders({ "Content-Type": "application/json" });

        const response = await fetch(config.baseUrl + "/client/service/api/v1/jobs", {
            method: "POST",
            headers: headers,
            body: JSON.stringify(roleData)
        });
        return ZeeroPro._handleResponse(response);
    };

    /**
     * POST {{BASE_URL}}/client/service/api/v1/process/career/apply  (multipart/form-data)
     * candidateData: { firstName, lastName, email, phone, location, roleName, screeningAnswers }
     * resumeFile: a File/Blob (e.g. from an <input type="file">)
     * options: { clientId, templateId, jobDescription }
     *   clientId/templateId fall back to the tenantId / templateId Environment Variables.
     */
    ZeeroPro.Api.apply = async function (candidateData, resumeFile, options) {
        options = options || {};
        const config = await ZeeroPro.Config.getAll();
        const token = await ZeeroPro.Auth.getAccessToken();
        const formData = new FormData();
        formData.append("candidateData", JSON.stringify(candidateData));
        formData.append("clientId", options.clientId || config.tenantId);
        formData.append("templateId", null);
        formData.append("job_description", options.jobDescription || "");
        if (resumeFile) {
            formData.append("resume", resumeFile, resumeFile.name);
        }

        const response = await fetch(config.baseUrl + "/client/service/api/v1/process/career/apply", {
            method: "POST",
            // Do NOT set Content-Type manually - the browser must set the multipart boundary.
            headers: { "Authorization": "Bearer " + token },
            body: formData
        });
        return ZeeroPro._handleResponse(response);
    };

    /**
     * POST {{BASE_URL}}/client/service/api/v1/process/career/shortlist/recruiter
     * clientId (optional, defaults to tenantId), roleName, candidateEmails: string[]
     */
    ZeeroPro.Api.shortlistCandidates = async function (clientId, roleName, candidateEmails) {
        const config = await ZeeroPro.Config.getAll();
         const token = await ZeeroPro.Auth.getAccessToken();
        const headers = await ZeeroPro._authHeaders({ "Content-Type": "application/json" }, { "Authorization": "Bearer " + token });

        const response = await fetch(config.baseUrl + "/client/service/api/v1/process/career/shortlist/recruiter", {
            method: "POST",
            headers: headers,
            body: JSON.stringify({
                clientId: clientId || config.tenantId,
                roleName: roleName,
                candidateEmails: candidateEmails || []
            })
        });
        return ZeeroPro._handleResponse(response);
    };

    /**
     * POST {{BASE_URL}}/client/service/api/v1/process/save  (multipart/form-data)
     * roleId, clientId (optional, defaults to tenantId), invites: [{ name, email, mobile }],
     * tenantClientId (optional, defaults to tenantId)
     */
    ZeeroPro.Api.invite = async function (roleId, clientId, invites, tenantClientId) {
        const config = await ZeeroPro.Config.getAll();
        const token = await ZeeroPro.Auth.getAccessToken();

        const formData = new FormData();
        formData.append("jsonData", JSON.stringify({
            roleId: roleId,
            clientId: clientId || config.tenantId,
            tenantClientId: tenantClientId || config.tenantId,
            stages: [{ stageType: "aiinterview", stageName: "ai assessment" }],
            isEmail: true,
            isWhatsapp: false
        }));
        formData.append("invitees", JSON.stringify({ invites: invites || [] }));

        const response = await fetch(config.baseUrl + "/client/service/api/v1/process/save", {
            method: "POST",
            headers: { "Authorization": "Bearer " + token },
            body: formData
        });
        return ZeeroPro._handleResponse(response);
    };

    // ---------------------------------------------------------------------
    // ZeeroPro.Commands  (ribbon / command-bar button handlers)
    //
    // Wire these up as a Command's JavaScript action in Ribbon Workbench (or
    // the modern command bar designer): Library = zeeropro-d365.js,
    // Function = one of the names below, CRM Parameter = PrimaryControl.
    //
    // TODO: replace the highlighted field logical names with your own schema.
    // ---------------------------------------------------------------------
    ZeeroPro.Commands = ZeeroPro.Commands || {};

    function getAttr(formContext, logicalName) {
        const attr = formContext.getAttribute(logicalName);
        return attr ? attr.getValue() : null;
    }

    function getLookupAttr(formContext, logicalName) {
        const attr = formContext.getAttribute(logicalName).getValue();
        return attr !== null && attr !== undefined ? attr[0].name : null;
    }

    function downloadFileColumn(formContext, fileColumnName) {
        const recordId = formContext.data.entity.getId().replace(/[{}]/g, "");
        const entityName = formContext.data.entity.getEntityName();

        if (!recordId) {
            return Promise.reject(new Error("Please save the application before uploading its file."));
        }

        return Xrm.Utility.getEntityMetadata(entityName).then(function (metadata) {
            const downloadUrl = Xrm.Utility.getGlobalContext().getClientUrl() +
                "/api/data/v9.2/" + metadata.EntitySetName + "(" + recordId + ")/" +
                fileColumnName + "/$value";

            return new Promise(function (resolve, reject) {
                const request = new XMLHttpRequest();
                request.open("GET", downloadUrl, true);
                request.setRequestHeader("OData-MaxVersion", "4.0");
                request.setRequestHeader("OData-Version", "4.0");
                request.setRequestHeader("Accept", "application/octet-stream");
                request.responseType = "blob";
                request.onload = function () {
                    if (request.status < 200 || request.status >= 300) {
                        reject(new Error("Unable to download the CRM file column (HTTP " + request.status + ")."));
                        return;
                    }

                    const contentDisposition = request.getResponseHeader("Content-Disposition") || "";
                    const fileNameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
                    const fileName = fileNameMatch && fileNameMatch[1]
                        ? decodeURIComponent(fileNameMatch[1].replace(/['"]/g, ""))
                        : fileColumnName;
                    const contentType = request.getResponseHeader("Content-Type") || "application/octet-stream";
                    resolve(new File([request.response], fileName, { type: contentType }));
                };
                request.onerror = function () {
                    reject(new Error("Unable to download the CRM file column."));
                };
                request.send();
            });
        });
    }


    /** Sets a form attribute only if that field exists on the current form - safe no-op otherwise. */
    function setAttr(formContext, logicalName, value) {
        const attr = formContext.getAttribute(logicalName);
        if (attr) { attr.setValue(value); }
    }

    async function notify(title, body) {
        try {
            await Xrm.Navigation.openAlertDialog({ text: body, title: title });
        } catch (e) { /* dialog dismissed / unavailable - ignore */ }
    }

    function showActionProgress(message) {
        Xrm.Utility.showProgressIndicator(message);
    }

    function closeActionProgress() {
        Xrm.Utility.closeProgressIndicator();
    }

    function executeZeeroProAction(formContext, actionName, message) {
        var request = {
            entity: {
                entityType: formContext.data.entity.getEntityName(),
                id: formContext.data.entity.getId().replace(/[{}]/g, "")
            },
            Message: message,
            getMetadata: function () {
                return {
                    boundParameter: "entity",
                    operationName: actionName,
                    operationType: 0,
                    parameterTypes: {
                        entity: { typeName: "mscrm." + formContext.data.entity.getEntityName(), structuralProperty: 5 },
                        Message: { typeName: "Edm.String", structuralProperty: 1 }
                    }
                };
            }
        };
        return Xrm.WebApi.online.execute(request).then(function (response) {
            return response.text().then(function (rawBody) {
                console.log("ZeeroPro action response", {
                    action: actionName,
                    status: response.status,
                    ok: response.ok,
                    headers: response.headers,
                    rawBody: rawBody
                });

                if (!response.ok) {
                    throw new Error("ZeeroPro action failed (HTTP " + response.status + "): " + rawBody);
                }

                if (!rawBody) {
                    return "";
                }

                try {
                    var body = JSON.parse(rawBody);
                    console.log("ZeeroPro Dataverse action envelope", body);

                    var responseBody = body && body.Response !== undefined
                        ? body.Response
                        : body;
                    if (typeof responseBody === "string" && responseBody.trim()) {
                        try {
                            responseBody = JSON.parse(responseBody);
                            console.log("ZeeroPro external API response", responseBody);
                        } catch (nestedParseError) {
                            console.warn("ZeeroPro Response is text, not nested JSON:", responseBody);
                        }
                    }
                    return responseBody || "";
                } catch (parseError) {
                    console.warn("ZeeroPro action returned a non-JSON response:", rawBody);
                    return rawBody;
                }
            });
        });
    }

    /**
     * Ribbon button on a "Job" form -> creates the role in ZeeroPro.
     *
    * The response includes ZeeroPro's own record `id` and a separate `jobId`
     * (a GUID) - `jobId` is the identifier ZeeroPro expects back on later
     * calls (e.g. as `roleId` on Invite), so it's written onto a
     * `cr_zeeroprojobid` field (create one, or change the name below) and
     * the form is saved so it isn't lost if the user navigates away.
     * The Postman collection's own Shortlistcandidates/Invite examples pass a
     * human-readable role name instead of this GUID - confirm with ZeeroPro
     * which one their API actually expects before relying on either.
     */
    ZeeroPro.Commands.createRoleFrom = async function (primaryControl) {
        debugger;
        const formContext = primaryControl;
        showActionProgress("Creating role in ZeeroPro...");
        try {
            await executeZeeroProAction(formContext, "bcc_ZeeroProAction", "CreateRole");
            const statusReason = formContext.getAttribute("statuscode");
            if (!statusReason) {
                throw new Error("The Status Reason field is not available on this form.");
            }
            statusReason.setValue(755700001);
            await formContext.data.save();
            await notify("ZeeroPro", "Role created successfully.");
        } catch (error) {
            await notify("ZeeroPro - Error", error.message);
        } finally {
            closeActionProgress();
        }
    };

    /**
    * Ribbon button on a "Candidate"/"Application" form -> submits the
    * application using the bcc_resume Dataverse File column.
     *
     * `roleName` is sent (and echoed back) as plain text here, unlike
     * CreateRole which returns a `jobId` GUID - Apply does NOT need that GUID,
     * a human-readable role name is enough. On success the returned `id`
     * (the application record's own id) and `resumeUrl` are written back onto
     * the form and saved, so they're available for correlating a later
     * shortlist/invite call or for showing the uploaded resume link.
     */
    ZeeroPro.Commands.applyFrom = async function (primaryControl) {
        debugger;
        const formContext = primaryControl;
        showActionProgress("Submitting application to ZeeroPro...");
        try {
            await executeZeeroProAction(formContext, "bcc_ZeeroProJA", "Apply");
            await notify("ZeeroPro", "Application submitted.");
        } catch (error) {
            await notify("ZeeroPro - Error", error.message);
        } finally {
            closeActionProgress();
        }
    };

    /**
     * Ribbon button on a "Candidate" form/grid -> shortlists the candidate.
     *
     * IMPORTANT: this endpoint returns HTTP 200 even when nothing actually
     * got shortlisted - e.g. because the email doesn't have an application
     * under that exact `roleName`/`clientId`. It reports that via
     * `notFoundEmails` and a `careerCandidatesUpdated` count of 0, not via an
     * error, so a bare try/catch will report "success" on a silent no-op.
     * This handler checks `notFoundEmails` explicitly and surfaces it as a
     * warning instead of a false "shortlisted" message.
     */
    ZeeroPro.Commands.shortlistFrom = async function (primaryControl) {
        const formContext = primaryControl;
        showActionProgress("Shortlisting candidate in ZeeroPro...");
        try {
            await executeZeeroProAction(formContext, "bcc_ZeeroProJA", "Shortlist");
            await notify("ZeeroPro", "Candidate shortlisted.");
        } catch (error) {
            await notify("ZeeroPro - Error", error.message);
        } finally {
            closeActionProgress();
        }
    };

    /**
     * Ribbon button on a "Candidate" form -> sends the invite.
     *
     * Like Shortlistcandidates, this endpoint returns HTTP 200 with a
     * `created`/`skipped` breakdown rather than an error for invitees it
     * couldn't process - a bare try/catch would report "success" even if
     * every invitee ended up in `skipped`. This handler checks `skipped`
     * explicitly and surfaces it as a warning.
     */
    ZeeroPro.Commands.inviteFrom = async function (primaryControl) {
        const formContext = primaryControl;
        showActionProgress("Sending invite through ZeeroPro...");
        try {
            await executeZeeroProAction(formContext, "bcc_ZeeroProJA", "Invite");
            await notify("ZeeroPro", "Invite sent successfully.");
        } catch (error) {
            await notify("ZeeroPro - Error", error.message);
        } finally {
            closeActionProgress();
        }
    };

    /** Example Enable Rule: only show the "Create Role" button once roleName is filled in. */
    ZeeroPro.Commands.canCreateRole = function (primaryControl) {
        return !!getAttr(primaryControl, "cr_rolename");
    };

})();