var BCC = BCC || {};
BCC.JobOfferRenewal = BCC.JobOfferRenewal || {};

BCC.JobOfferRenewal.IsRenewButtonVisible = function (primaryControl) {
    try {
        var formContext = primaryControl;
        var statecodeAttr = formContext.getAttribute("statecode");
        if (!statecodeAttr) return false;
        return statecodeAttr.getValue() === 1; // 1 = Inactive/Deactivated
    } catch (e) {
        console.error("BCC.JobOfferRenewal.IsRenewButtonVisible Error:", e);
        return false;
    }
};

BCC.JobOfferRenewal.CloneJobOffer = async function (primaryControl) {
    var formContext = primaryControl;

    try {
        var confirmResult = await Xrm.Navigation.openConfirmDialog(
            {
                title: "Renew Job Offer",
                text: "Are you sure you want to renew this Job Offer? A new renewed record will be created with all field data and documents copied.",
                confirmButtonLabel: "Yes, Renew",
                cancelButtonLabel: "Cancel"
            },
            { height: 220, width: 450 }
        );

        if (!confirmResult.confirmed) return;

        var sourceRecordId = formContext.data.entity.getId().replace(/[{}]/g, "");

        Xrm.Utility.showProgressIndicator("Renewing Job Offer. Please wait...");

        var sourceRecord = await Xrm.WebApi.retrieveRecord("bcc_joboffer", sourceRecordId);
        var simpleFields = [
            {
                "FieldName": "bcc_jobapplicationnumber",
                "FieldType": "string"
            },
            {
                "FieldName": "bcc_mailstatus",
                "FieldType": "multiselectoptionset"
            },
            {
                "FieldName": "bcc_parentjoboffer",
                "FieldType": "lookup"
            },
            {
                "FieldName": "bcc_joboffernumber",
                "FieldType": "string"
            },
            {
                "FieldName": "bcc_cancelreason",
                "FieldType": "string"
            },
            {
                "FieldName": "bcc_probationperiod",
                "FieldType": "optionset"
            },
            {
                "FieldName": "bcc_weeklyrest",
                "FieldType": "multiselectoptionset"
            },
            {
                "FieldName": "bcc_canceldocumentuploaddate",
                "FieldType": "datetime"
            },
            {
                "FieldName": "bcc_noticeperiod",
                "FieldType": "optionset"
            },
            {
                "FieldName": "bcc_substagestatus",
                "FieldType": "optionset"
            },
            {
                "FieldName": "bcc_applicationfrom_",
                "FieldType": "optionset"
            },
            {
                "FieldName": "bcc_gender",
                "FieldType": "optionset"
            },
            {
                "FieldName": "bcc_applicationform",
                "FieldType": "optionset"
            },
            {
                "FieldName": "bcc_approval",
                "FieldType": "optionset"
            },
            {
                "FieldName": "bcc_givennames",
                "FieldType": "string"
            },
            {
                "FieldName": "bcc_manpowerrequest",
                "FieldType": "lookup"
            },
            {
                "FieldName": "bcc_newcolumn",
                "FieldType": "string"
            },
            {
                "FieldName": "bcc_rejectremark",
                "FieldType": "string"
            },
            {
                "FieldName": "bcc_middlename",
                "FieldType": "string"
            },
            {
                "FieldName": "bcc_lastname",
                "FieldType": "string"
            },
            {
                "FieldName": "bcc_rejectdocuploaddate",
                "FieldType": "datetime"
            },
            {
                "FieldName": "bcc_dateofbirth",
                "FieldType": "datetime"
            },
            {
                "FieldName": "bcc_manpowerquota",
                "FieldType": "lookup"
            },
            {
                "FieldName": "bcc_address",
                "FieldType": "string"
            },
            {
                "FieldName": "bcc_contact",
                "FieldType": "lookup"
            },
            {
                "FieldName": "bcc_passportexpirydate",
                "FieldType": "datetime"
            },
            {
                "FieldName": "bcc_nationality_",
                "FieldType": "string"
            },
            {
                "FieldName": "bcc_passportno",
                "FieldType": "string"
            },
            {
                "FieldName": "bcc_jobapplication",
                "FieldType": "lookup"
            },
            {
                "FieldName": "bcc_passportissuedate",
                "FieldType": "datetime"
            },
            {
                "FieldName": "bcc_designation",
                "FieldType": "lookup"
            },
            {
                "FieldName": "bcc_labourfeepaid",
                "FieldType": "boolean"
            },
            {
                "FieldName": "bcc_jobofferstatus",
                "FieldType": "optionset"
            },
            {
                "FieldName": "bcc_applyvisa",
                "FieldType": "optionset"
            },
            {
                "FieldName": "bcc_medicalapplicationfees",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_iloeinsurencefees",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_visafees",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_residencefees",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_emiratesidappfees",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_dateofarrival",
                "FieldType": "datetime"
            },
            {
                "FieldName": "bcc_passportuploaddate",
                "FieldType": "datetime"
            },
            {
                "FieldName": "bcc_degreecertificateuploaddate",
                "FieldType": "datetime"
            },
            {
                "FieldName": "bcc_passport2uploaddate",
                "FieldType": "datetime"
            },
            {
                "FieldName": "bcc_degreecertificate2uploaddate",
                "FieldType": "datetime"
            },
            {
                "FieldName": "bcc_passport3uploaddate",
                "FieldType": "datetime"
            },
            {
                "FieldName": "bcc_workerformuploaddate",
                "FieldType": "datetime"
            },
            {
                "FieldName": "bcc_otherdocumentuploaddate",
                "FieldType": "datetime"
            },
            {
                "FieldName": "bcc_cancellationformuploaddate",
                "FieldType": "datetime"
            },
            {
                "FieldName": "bcc_photouploaddate",
                "FieldType": "datetime"
            },
            {
                "FieldName": "bcc_basic",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_allowance",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_hrajoboffer",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_totalsalary",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_universityname",
                "FieldType": "string"
            },
            {
                "FieldName": "bcc_degreetype",
                "FieldType": "string"
            },
            {
                "FieldName": "bcc_studymajors",
                "FieldType": "string"
            },
            {
                "FieldName": "bcc_faculty",
                "FieldType": "string"
            },
            {
                "FieldName": "bcc_degreestartdate",
                "FieldType": "integer"
            },
            {
                "FieldName": "bcc_degreeenddate",
                "FieldType": "integer"
            },
            {
                "FieldName": "bcc_graduationyear",
                "FieldType": "integer"
            },
            {
                "FieldName": "bcc_contract",
                "FieldType": "optionset"
            },
            {
                "FieldName": "statecode",
                "FieldType": "optionset"
            },
            {
                "FieldName": "bcc_nationality",
                "FieldType": "lookup"
            },
            {
                "FieldName": "bcc_totalpremiummf",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_totalamountla",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_totalchargela",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_grandtotal",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_visachargevf",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_amount",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_amountchargecs",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_totalchargemf",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_amountme",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_totaleid",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_totalchargeeid",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_iloeinsurencechargeiloe",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_amountiloe",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_fine",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_totalamounttt",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_charge",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_grandtotalre",
                "FieldType": "money"
            },
            {
                "FieldName": "bcc_totalchargere",
                "FieldType": "money"
            }
        ]

        var cloneData = {};
        // Do NOT set statecode/statuscode — let D365 assign the default Active status automatically.
        cloneData["bcc_contract"] = 755700001;
        cloneData["bcc_Parentjoboffer@odata.bind"] =  "/bcc_joboffers(" + sourceRecordId + ")";
        var lookupMappings = {
            "bcc_parentjoboffer": { navigationProperty: "bcc_Parentjoboffer", entitySetName: "bcc_joboffers" },
            "bcc_manpowerrequest": { navigationProperty: "bcc_ManPowerRequest", entitySetName: "bcc_manpowerrequests" },
            "bcc_manpowerquota": { navigationProperty: "bcc_ManPowerQuota", entitySetName: "bcc_manpowerquotas" },
            "bcc_contact": { navigationProperty: "bcc_Contact", entitySetName: "contacts" },
            "bcc_jobapplication": { navigationProperty: "bcc_JobApplication", entitySetName: "bcc_jobapplications" },
            "bcc_designation": { navigationProperty: "bcc_Designation", entitySetName: "bcc_designations" },
            "bcc_nationality": { navigationProperty: "bcc_Nationality", entitySetName: "msdyn_regions" }
        };

        simpleFields.forEach(function (field) {
            if (field.FieldName === "statecode" || field.FieldName === "bcc_contract") {
                return;
            }

            if (field.FieldType === "lookup") {
                var lookupId = sourceRecord["_" + field.FieldName + "_value"];
                if (!lookupId && typeof sourceRecord[field.FieldName] === "string") {
                    lookupId = sourceRecord[field.FieldName];
                }

                if (lookupId) {
                    var mapping = lookupMappings[field.FieldName];
                    if (!mapping) {
                        throw new Error("Lookup mapping is missing for field " + field.FieldName + ".");
                    }

                    cloneData[mapping.navigationProperty + "@odata.bind"] =
                        "/" + mapping.entitySetName + "(" + lookupId.replace(/[{}]/g, "") + ")";
                }
                return;
            }

            var value = sourceRecord[field.FieldName];
            if (value !== null && value !== undefined) {
                if (field.FieldType === "multiselectoptionset" && Array.isArray(value)) {
                    value = value.join(",");
                } else if (field.FieldType === "multiselectoptionset" && typeof value === "string") {
                    value = value.split(",").map(function (option) {
                        return option.trim();
                    }).filter(function (option) {
                        return option !== "";
                    }).join(",");
                }
                cloneData[field.FieldName] = value;
            }
        });

        // Step 1: Create clone record
        var newRecord = await Xrm.WebApi.createRecord("bcc_joboffer", cloneData);
        var newRecordId = newRecord.id.replace(/[{}]/g, "");

        var fileMappings = [
            { source: "bcc_canceldocument", target: "bcc_canceldocument", isImage: false },
            { source: "bcc_rejectdoc1", target: "bcc_rejectdoc1", isImage: false },
            { source: "bcc_uploadmadicaltest", target: "bcc_uploadmadicaltest", isImage: false },
            { source: "bcc_iloedocument", target: "bcc_iloedocument", isImage: false },
            { source: "bcc_tawjiitrainingdocument", target: "bcc_tawjiitrainingdocument", isImage: false },
            { source: "bcc_residencedocument", target: "bcc_residencedocument", isImage: false },
            { source: "bcc_attatchairticket", target: "bcc_attatchairticket", isImage: false },
            { source: "bcc_passport", target: "bcc_passport", isImage: false },
            { source: "bcc_degreecertificate", target: "bcc_degreecertificate", isImage: false },
            { source: "bcc_passport2jo", target: "bcc_passport2jo", isImage: false },
            { source: "bcc_degreecertificate2", target: "bcc_degreecertificate2", isImage: false },
            { source: "bcc_passport3", target: "bcc_passport3", isImage: false },
            { source: "bcc_workerform", target: "bcc_workerform", isImage: false },
            { source: "bcc_otherdocument", target: "bcc_otherdocument", isImage: false },
            { source: "bcc_cancellationform", target: "bcc_cancellationform", isImage: false },
            { source: "bcc_image", target: "bcc_image", isImage: true },
            { source: "bcc_mohreletter", target: "bcc_mohreletter", isImage: false },
            { source: "bcc_joboffer", target: "bcc_joboffer", isImage: false },
            { source: "bcc_signcontract", target: "bcc_signcontract", isImage: false },
            { source: "bcc_employeecontract", target: "bcc_employeecontract", isImage: false },
            { source: "bcc_signedjoboffer", target: "bcc_signedjoboffer", isImage: false },
            { source: "bcc_preapprovalworkpermit", target: "bcc_preapprovalworkpermit", isImage: false },
            { source: "bcc_insurancemolattachment", target: "bcc_insurancemolattachment", isImage: false },
            { source: "bcc_labourfee", target: "bcc_labourfee", isImage: false },
            { source: "bcc_visafee", target: "bcc_visafee", isImage: false },
            { source: "bcc_changestatus", target: "bcc_changestatus", isImage: false },
            { source: "bcc_medicalresult", target: "bcc_medicalresult", isImage: false },
            { source: "bcc_medicalfee", target: "bcc_medicalfee", isImage: false },
            { source: "bcc_eid", target: "bcc_eid", isImage: false },
            { source: "bcc_tawjeetraining", target: "bcc_tawjeetraining", isImage: false }
        ]

        var availableFileMappings = fileMappings.filter(function (mapping) {
            return sourceRecord[mapping.source+"_name"] !== null &&
                sourceRecord[mapping.source+"_name"] !== undefined;
        });

        var copyPromises = availableFileMappings.map(function (mapping) {
            return BCC.JobOfferRenewal.CopyFileField(
                "bcc_joboffer",
                sourceRecordId,
                mapping.source,
                "bcc_joboffer",
                newRecordId,
                mapping.target,
                mapping.isImage
            );
        });

        Promise.all(copyPromises).then(
            function () {
                Xrm.Utility.closeProgressIndicator();
                Xrm.Navigation.openAlertDialog({
                    title: "Job Offer Renewed",
                    text: "Your Job Offer has been renewed successfully. Opening the renewed record now."
                }).then(function () {
                    Xrm.Navigation.openForm({
                        entityName: "bcc_joboffer",
                        entityId: newRecordId
                    });
                });
            },
            function (error) {
                Xrm.Utility.closeProgressIndicator();
                Xrm.Navigation.openAlertDialog({
                    title: "Job Offer Renewed",
                    text: "Your Job Offer has been renewed. However, some documents could not be copied.\n\nError: " + error
                }).then(function () {
                    Xrm.Navigation.openForm({
                        entityName: "bcc_joboffer",
                        entityId: newRecordId
                    });
                });
            }
        );

    } catch (error) {
        Xrm.Utility.closeProgressIndicator();
        console.error("BCC.JobOfferRenewal.CloneJobOffer Error:", error);
        Xrm.Navigation.openAlertDialog({
            title: "Renewal Failed",
            text: "An error occurred while renewing the Job Offer:\n\n" + (error.message || JSON.stringify(error))
        });
    }
};

BCC.JobOfferRenewal.CopyFileField = function (
    sourceEntityName,
    sourceRecordId,
    sourceFieldName,
    targetEntityName,
    targetRecordId,
    targetFieldName,
    isImage
) {
    return new Promise(function (resolve, reject) {
        Xrm.Utility.getEntityMetadata(sourceEntityName).then(
            function (sourceEntityData) {
                var sourceEntitySetName = sourceEntityData.EntitySetName;

                var downloadUrl =
                    Xrm.Utility.getGlobalContext().getClientUrl() +
                    "/api/data/v9.2/" + sourceEntitySetName + "(" + sourceRecordId + ")/" + sourceFieldName + "/$value";

                var downloadReq = new XMLHttpRequest();
                downloadReq.open("GET", downloadUrl, true);
                downloadReq.setRequestHeader("OData-MaxVersion", "4.0");
                downloadReq.setRequestHeader("OData-Version", "4.0");
                downloadReq.setRequestHeader("Accept", "application/octet-stream");
                downloadReq.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
                downloadReq.responseType = "blob";

                downloadReq.onload = function () {
                    if (downloadReq.status === 200) {
                        var blob = downloadReq.response;
                        var contentDisposition = downloadReq.getResponseHeader("Content-Disposition");
                        var contentType = downloadReq.getResponseHeader("Content-Type") || blob.type || "application/octet-stream";
                        var fileName = sourceFieldName + "_copy";

                        if (contentDisposition) {
                            var fileNameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
                            if (fileNameMatch && fileNameMatch[1]) {
                                fileName = decodeURIComponent(fileNameMatch[1].replace(/['"]/g, ""));
                            }
                        }

                        if (isImage) {
                            var reader = new FileReader();
                            reader.onload = function (e) {
                                var img = new Image();
                                img.onload = function () {
                                    var canvas = document.createElement("canvas");
                                    canvas.width = 600;
                                    canvas.height = 600;
                                    var ctx = canvas.getContext("2d");
                                    ctx.imageSmoothingEnabled = true;
                                    ctx.imageSmoothingQuality = "high";
                                    ctx.drawImage(img, 0, 0, 600, 600);
                                    canvas.toBlob(function (resizedBlob) {
                                        BCC.JobOfferRenewal.UploadFile(
                                            targetEntityName, targetRecordId, targetFieldName,
                                            fileName, resizedBlob, contentType, resolve, reject
                                        );
                                    }, contentType, 1.0);
                                };
                                img.src = e.target.result;
                            };
                            reader.readAsDataURL(blob);
                        } else {
                            BCC.JobOfferRenewal.UploadFile(
                                targetEntityName, targetRecordId, targetFieldName,
                                fileName, blob, contentType, resolve, reject
                            );
                        }

                    } else if (downloadReq.status === 404) {
                        console.log("No file on field: " + sourceFieldName + ". Skipping.");
                        resolve("No file");
                    } else {
                        console.error("Download failed for " + sourceFieldName + ": HTTP " + downloadReq.status);
                        reject("Download failed for " + sourceFieldName + ": HTTP " + downloadReq.status);
                    }
                };

                downloadReq.onerror = function () {
                    reject("Network error downloading field: " + sourceFieldName);
                };

                downloadReq.send();
            },
            function (error) {
                reject("Metadata error: " + error.message);
            }
        );
    });
};

BCC.JobOfferRenewal.UploadFile = function (
    targetEntityName, targetRecordId, targetFieldName,
    fileName, blob, contentType, resolve, reject
) {
    Xrm.Utility.getEntityMetadata(targetEntityName).then(
        function (targetEntityData) {
            var targetEntitySetName = targetEntityData.EntitySetName;

            var uploadUrl =
                Xrm.Utility.getGlobalContext().getClientUrl() +
                "/api/data/v9.2/" + targetEntitySetName + "(" + targetRecordId + ")/" + targetFieldName;

            var uploadReq = new XMLHttpRequest();
            uploadReq.open("PATCH", uploadUrl, true);
            uploadReq.setRequestHeader("OData-MaxVersion", "4.0");
            uploadReq.setRequestHeader("OData-Version", "4.0");
            uploadReq.setRequestHeader("Content-Type", contentType);
            uploadReq.setRequestHeader("x-ms-file-name", fileName);
            uploadReq.setRequestHeader("Content-Range", "bytes 0-" + (blob.size - 1) + "/" + blob.size);

            uploadReq.onload = function () {
                if (uploadReq.status === 204 || uploadReq.status === 200) {
                    console.log("File uploaded successfully: " + targetFieldName);
                    resolve("Success: " + targetFieldName);
                } else {
                    console.error("Upload failed for " + targetFieldName + ": HTTP " + uploadReq.status + " - " + uploadReq.responseText);
                    reject("Upload failed for " + targetFieldName + ": HTTP " + uploadReq.status);
                }
            };

            uploadReq.onerror = function () {
                reject("Network error uploading to field: " + targetFieldName);
            };

            uploadReq.send(blob);
        },
        function (error) {
            reject("Target metadata error: " + error.message);
        }
    );
};