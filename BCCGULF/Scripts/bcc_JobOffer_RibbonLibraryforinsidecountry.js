function onFormLoadd(executionContext) {
    var formContext = executionContext.getFormContext();
    var applyVisaAttr = formContext.getAttribute("bcc_applyvisa");
    if (applyVisaAttr) {
        applyVisaAttr.addOnChange(function () {
            formContext.ui.refreshRibbon(true);
        });
    }
    formContext.ui.refreshRibbon(true);
}
function showhideEmail_btnn(primaryControl) {
    var formContext = primaryControl;
    var formId = formContext.ui.formSelector.getCurrentItem().getId().replace(/[{}]/g, "").toLowerCase();

    if (formId !== "5b8da095-5ef0-f011-8406-70a8a52260b1") {
        return false;
    }

    var applyVisaAttr = formContext.getAttribute("bcc_applyvisa");
    if (!applyVisaAttr) return false;

    var applyVisaValue = applyVisaAttr.getValue();

    return applyVisaValue === 755700000 || applyVisaValue === "755700000" ||
           applyVisaValue === 755700001 || applyVisaValue === "755700001";
}

function getEmailTemplatee(formContext) {
    var candidateName = "";
    var candidateAttr = formContext.getAttribute("bcc_givennames");
    if (candidateAttr && candidateAttr.getValue()) {
        candidateName = candidateAttr.getValue();
    }

    var applyVisaAttr = formContext.getAttribute("bcc_applyvisa");
    var applyVisaValue = applyVisaAttr ? applyVisaAttr.getValue() : null;

    if (applyVisaValue === 755700001 || applyVisaValue === "755700001") {
        return {
            subject: "E-Visa for Outside Country Candidate \u2013 Joining Date",
            body: "<p>Dear Team,</p>" +
                  "<p>Please find attached the e-visa for <strong>" + candidateName + "</strong>. " +
                  "Kindly let us know his earliest joining date.</p>" +
                  "<p>If you have any questions or require further clarification, please feel free to reach out.</p>" +
                  "<p>Best regards,<br>BCC HR Team</p>"
        };
    }

    return {
        subject: "E-Visa for Inside Country Candidate \u2013 Joining Date",
        body: "<p>Dear Team,</p>" +
              "<p>Please find attached the e-visa for <strong>" + candidateName + "</strong>. " +
              "Kindly let us know his earliest joining date.</p>" +
              "<p>If you have any questions or require further clarification, please feel free to reach out.</p>" +
              "<p>Best regards,<br>BCC HR Team</p>"
    };
}

// ↓↓↓ YE FUNCTION ADD KARO YAHAN ↓↓↓
function getAttachFieldsByFormm(formContext) {
    return ["bcc_visafee"];
}
// ↑↑↑ BUS ITNA ADD KARO ↑↑↑

var CC_ACCOUNTSS = [
    "/accounts(748796e7-c329-f111-8342-6045bd6a0293)",
    "/accounts(6990296b-7a27-f111-8342-6045bd6a0293)",
    "/accounts(5b176378-c329-f111-8342-6045bd6a0293)",
    "/accounts(3aebe5bc-c329-f111-8342-6045bd6a0293)",
    "/accounts(263ca8a2-c329-f111-8342-6045bd6a0293)",
    "/accounts(01fb4dd5-c329-f111-8342-6045bd6a0293)",
    "/systemusers(c3da9e67-fa9a-f111-b8db-6045bd6b31e9)"//MIshkat
];

function buildCcPartiest(formContext) {
    return CC_ACCOUNTSS.map(function (accountPath) {
        if (accountPath.startsWith("/systemusers(")) {
            return { "partyid_systemuser@odata.bind": accountPath, "participationtypemask": 3 };
        }
        return { "partyid_account@odata.bind": accountPath, "participationtypemask": 3 };
    });
}

function attachFilePromisee(fieldName, emailId, recordId) {
    return new Promise(function (resolve) {
        var clientUrl = Xrm.Utility.getGlobalContext().getClientUrl();
        var fileUrl = clientUrl + "/api/data/v9.2/bcc_joboffers(" + recordId + ")/" + fieldName + "?size=full";

        var req = new XMLHttpRequest();
        req.open("GET", fileUrl, true);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.responseType = "text";

        req.onload = function () {
            if (req.status === 404 || req.status === 204) {
                console.warn("No file: " + fieldName);
                resolve(); return;
            }
            if (req.status !== 200) {
                console.warn("Failed: " + fieldName + " | " + req.status);
                resolve(); return;
            }

            var rawFileName = req.getResponseHeader("x-ms-file-name") || fieldName;
            var fileName;
            try { fileName = decodeURIComponent(rawFileName); }
            catch (e) { fileName = rawFileName; }

            console.log("Filename from header: [" + fileName + "]");
            console.log("Content-Type: [" + req.getResponseHeader("content-type") + "]");
            console.log("Raw response (first 200): " + req.responseText.substring(0, 200));

            var base64String = null;
            var finalMime = null;

            try {
                var json = JSON.parse(req.responseText);
                console.log("JSON keys:", Object.keys(json));

                if (json.value) {
                    base64String = json.value;
                    console.log("Got base64 from json.value, length: " + base64String.length);
                }
                if (json["@odata.mediaContentType"]) {
                    finalMime = json["@odata.mediaContentType"];
                    console.log("MIME from odata: " + finalMime);
                }
            } catch (e) {
                console.warn("Not JSON, treating as raw base64");
                base64String = btoa(req.responseText);
            }

            if (!base64String) {
                console.error("Could not extract file data for: " + fieldName);
                resolve(); return;
            }

            if (!finalMime || finalMime === "application/octet-stream") {
                try {
                    var binaryStr = atob(base64String.substring(0, 100));
                    var firstBytes = [];
                    for (var i = 0; i < binaryStr.length; i++) {
                        firstBytes.push(binaryStr.charCodeAt(i));
                    }
                    console.log("First bytes:", firstBytes.slice(0, 10));
                    finalMime = detectMimeFromBytess(firstBytes, fileName);
                } catch (e) {
                    finalMime = getMimeTypeFromFileNamee(fileName);
                }
            }

            var ext = fileName.split('.').pop().toLowerCase();
            var knownExts = ["pdf", "jpg", "jpeg", "png", "gif", "doc", "docx", "xls", "xlsx", "ppt", "pptx", "txt", "csv", "zip", "bmp"];
            if (!knownExts.includes(ext)) {
                fileName = fileName.replace(/\.[^/.]+$/, "") + "." + getExtFromMimee(finalMime);
            } else {
                var expectedExt = getExtFromMimee(finalMime);
                if (expectedExt !== "bin" && ext !== expectedExt) {
                    console.warn("Wrong extension! Stored as ." + ext + " but actually ." + expectedExt);
                    fileName = fileName.replace(/\.[^/.]+$/, "") + "." + expectedExt;
                }
            }

            console.log("Final - Name: [" + fileName + "] | MIME: [" + finalMime + "]");

            var attachment = {
                "objectid_email@odata.bind": "/emails(" + emailId + ")",
                "objecttypecode": "email",
                "subject": fieldName,
                "filename": fileName,
                "body": base64String,
                "mimetype": finalMime
            };

            Xrm.WebApi.createRecord("activitymimeattachment", attachment).then(
                function () {
                    console.log("Attached: " + fileName);
                    resolve();
                },
                function (error) {
                    console.error("Error: " + error.message);
                    resolve();
                }
            );
        };

        req.onerror = function () {
            console.error("XHR error: " + fieldName);
            resolve();
        };

        req.send();
    });
}

function detectMimeFromBytess(bytes, fallbackFileName) {
    if (bytes[0] === 0x25 && bytes[1] === 0x50 && bytes[2] === 0x44 && bytes[3] === 0x46)
        return "application/pdf";
    if (bytes[0] === 0x89 && bytes[1] === 0x50 && bytes[2] === 0x4E && bytes[3] === 0x47)
        return "image/png";
    if (bytes[0] === 0xFF && bytes[1] === 0xD8 && bytes[2] === 0xFF)
        return "image/jpeg";
    if (bytes[0] === 0x47 && bytes[1] === 0x49 && bytes[2] === 0x46)
        return "image/gif";
    if (bytes[0] === 0x42 && bytes[1] === 0x4D)
        return "image/bmp";
    if (bytes[0] === 0x50 && bytes[1] === 0x4B)
        return "application/zip";
    if (bytes[0] === 0xD0 && bytes[1] === 0xCF)
        return "application/vnd.ms-excel";
    return getMimeTypeFromFileNamee(fallbackFileName);
}

function getMimeTypeFromFileNamee(fileName) {
    if (!fileName) return "application/octet-stream";
    var ext = fileName.split('.').pop().toLowerCase();
    var map = {
        "pdf":  "application/pdf",
        "jpg":  "image/jpeg",
        "jpeg": "image/jpeg",
        "png":  "image/png",
        "gif":  "image/gif",
        "bmp":  "image/bmp",
        "doc":  "application/msword",
        "docx": "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "xls":  "application/vnd.ms-excel",
        "xlsx": "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "ppt":  "application/vnd.ms-powerpoint",
        "pptx": "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "txt":  "text/plain",
        "csv":  "text/csv",
        "zip":  "application/zip"
    };
    return map[ext] || "application/octet-stream";
}

function getExtFromMimee(mimeType) {
    var map = {
        "application/pdf": "pdf",
        "image/jpeg": "jpg",
        "image/png": "png",
        "image/gif": "gif",
        "image/bmp": "bmp",
        "application/msword": "doc",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document": "docx",
        "application/vnd.ms-excel": "xls",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet": "xlsx",
        "application/vnd.ms-powerpoint": "ppt",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation": "pptx",
        "text/plain": "txt",
        "text/csv": "csv",
        "application/zip": "zip"
    };
    return map[mimeType] || "bin";
}

function tocontact_onclickk(primaryControl) {
    debugger;
    var formContext = primaryControl;
    var contact = formContext.getAttribute("bcc_contact").getValue();
    if (contact != null) {
        var contactId = contact[0].id.replace(/[{}]/g, "");
        var owner = Xrm.Utility.getGlobalContext().userSettings.userId.replace(/[{}]/g, "");
        var ownerId = owner;
        var recordId = formContext.data.entity.getId().replace(/[{}]/g, "");

        var fieldsToAttach = getAttachFieldsByFormm(formContext);
        var template = getEmailTemplatee(formContext);

        var parties = [
            {
                "partyid_contact@odata.bind": "/contacts(" + contactId + ")",
                "participationtypemask": 2
            },
            {
                "partyid_systemuser@odata.bind": "/systemusers(" + ownerId + ")",
                "participationtypemask": 1
            }
        ].concat(buildCcPartiest(formContext));

        var emailRecord = {
            "subject": template.subject,
            "description": template.body,
            "directioncode": true,
            "email_activity_parties": parties
        };

        Xrm.WebApi.createRecord("email", emailRecord).then(
            function (result) {
                var emailId = result.id.replace(/[{}]/g, "");
                console.log("Email created: " + emailId);

                var attachPromises = fieldsToAttach.map(function (fieldName) {
                    return attachFilePromisee(fieldName, emailId, recordId);
                });

                Promise.all(attachPromises).then(function () {
                    Xrm.Navigation.openForm({ entityName: "email", entityId: emailId });
                    updateMailStatuss(recordId, 755700000);
                }).catch(function () {
                    Xrm.Navigation.openForm({ entityName: "email", entityId: emailId });
                    updateMailStatuss(recordId, 755700000);
                });
            },
            function (error) {                

                let errorOptions = {
                    details:error.message,
                    errorCode: 404,
                    message: 'Failed to create email',
                };
                
                Xrm.Navigation.openErrorDialog(errorOptions).then(
                    function () {
                        // perform operations on error dialog close
                        
                    },
                    function (error) {
                        console.log(error.message);
                        // handle error conditions
                    }
                );
            }
        );
    }
}

function toAgency_onclickk(primaryControl) {
    debugger;
    var formContext = primaryControl;
    var jobAppAttr = formContext.getAttribute("bcc_jobapplication");
    if (!jobAppAttr) {
        console.log("Job Application field not found on form");
        return;
    }
    var jobApp = jobAppAttr.getValue();
    if (!jobApp) return;
    var jobAppId = jobApp[0].id.replace(/[{}]/g, "");
    var recordId = formContext.data.entity.getId().replace(/[{}]/g, "");
    var owner = Xrm.Utility.getGlobalContext().userSettings.userId.replace(/[{}]/g, "");
    var ownerId = owner;

    var fieldsToAttach = getAttachFieldsByFormm(formContext);
    var template = getEmailTemplatee(formContext);

    Xrm.WebApi.retrieveRecord("bcc_jobapplication", jobAppId, "?$select=_bcc_manpowerrequest_value")
        .then(function (jobAppResult) {
            var manpowerId = jobAppResult["_bcc_manpowerrequest_value"];
            if (!manpowerId) return;
            return Xrm.WebApi.retrieveRecord("bcc_manpowerrequest", manpowerId, "?$select=_bcc_agency_value");
        })
        .then(function (manpowerResult) {
            if (!manpowerResult) return;
            var accountId = manpowerResult["_bcc_agency_value"];
            if (!accountId) return;

            var parties = [
                {
                    "partyid_account@odata.bind": "/accounts(" + accountId + ")",
                    "participationtypemask": 2
                },
                {
                    "partyid_systemuser@odata.bind": "/systemusers(" + ownerId + ")",
                    "participationtypemask": 1
                }
            ].concat(buildCcPartiest(formContext));

            var emailRecord = {
                "subject": template.subject,
                "description": template.body,
                "directioncode": true,
                "email_activity_parties": parties
            };

            return Xrm.WebApi.createRecord("email", emailRecord).then(function (result) {
                var emailId = result.id.replace(/[{}]/g, "");
                console.log("Email created: " + emailId);

                var attachPromises = fieldsToAttach.map(function (fieldName) {
                    return attachFilePromisee(fieldName, emailId, recordId);
                });

                return Promise.all(attachPromises).then(function () {
                    Xrm.Navigation.openForm({ entityName: "email", entityId: emailId });
                    updateMailStatuss(recordId, 755700001);
                }).catch(function () {
                    Xrm.Navigation.openForm({ entityName: "email", entityId: emailId });
                    updateMailStatuss(recordId, 755700001);
                });
            });
        })
        .catch(function (error) {
            console.log("Error: " + error.message);
        });
}

function updateMailStatuss(recordId, newValue) {
    Xrm.WebApi.retrieveRecord("bcc_joboffer", recordId, "?$select=bcc_mailstatus")
        .then(function (result) {
            var currentValues = result["bcc_mailstatus"];
            console.log("Current raw value:", currentValues);

            var newValuesArray = [];

            if (currentValues && currentValues !== null) {
                var existing = currentValues.toString().split(",").map(function (v) {
                    return parseInt(v.trim());
                });
                newValuesArray = existing;
            }

            if (newValuesArray.indexOf(newValue) === -1) {
                newValuesArray.push(newValue);
            } else {
                console.log("Already set: " + newValue);
                return;
            }

            console.log("Updating to:", newValuesArray);

            return Xrm.WebApi.updateRecord("bcc_joboffer", recordId, {
                "bcc_mailstatus": newValuesArray.join(",")
            });
        })
        .then(function () {
            console.log("bcc_mailstatus updated: " + newValue);
        })
        .catch(function (error) {
            console.error("Failed: " + error.message);
        });
}