var CancelOnDeactivate = {

    onCancelButtonClick: function (primaryControl) {

        var formContext = null;
        var recordId = null;
        var entityName = null;

        if (primaryControl && primaryControl.data && primaryControl.data.entity) {
            formContext = primaryControl;
            recordId = formContext.data.entity.getId();
            entityName = formContext.data.entity.getEntityName();
        }
        else if (primaryControl && primaryControl.getGrid) {
            Xrm.Navigation.openAlertDialog({
                title: "Open the Job Offer",
                text: "To cancel and switch directly to the Cancellation BPF, open the Job Offer record and run this action from its form."
            });
            return;
        }
        else {
            Xrm.Navigation.openAlertDialog({
                title: "Error",
                text: "Unsupported context."
            });
            return;
        }

        if (!recordId) {
            Xrm.Navigation.openAlertDialog({
                title: "Save Required",
                text: "Please save the record before cancelling."
            });
            return;
        }

        Xrm.Navigation.openConfirmDialog({
            title: "Confirm Cancellation",
            text: "Are you sure you want to cancel this Job Offer?"
        }).then(function (result) {
            if (!result.confirmed) return;
            CancelOnDeactivate.showCancelPopup(formContext, entityName, recordId);
        });
    },

    // ===============================
    // GET CORRECT DOCUMENT CONTEXT
    // ===============================
    getTargetDoc: function () {
        try {
            return top.document.body ? top.document : document;
        } catch (e) {
            return document;
        }
    },

    // ===============================
    // ON CANCEL DOCUMENT CHANGE (for delete case)
    // ===============================
    onCancelDocUpload: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var fileAttr = formContext.getAttribute("bcc_canceldocument");
        var dateAttr = formContext.getAttribute("bcc_canceldocumentuploaddate");

        if (!fileAttr || !dateAttr) return;

        var fileValue = fileAttr.getValue();

        if (fileValue != null && fileValue !== "") {
            dateAttr.setValue(new Date());
            dateAttr.setSubmitMode("always");
        } else {
            dateAttr.setValue(null);
            dateAttr.setSubmitMode("always");

            var recordId = formContext.data.entity.getId().replace(/[{}]/g, "");
            var entityName = formContext.data.entity.getEntityName();

            Xrm.WebApi.updateRecord(entityName, recordId, {
                bcc_canceldocumentuploaddate: null
            }).catch(function (err) {
                Xrm.Navigation.openAlertDialog({
                    title: "Error",
                    text: "Failed to clear upload date: " + (err.message || JSON.stringify(err))
                });
            });
        }
    },

    // ===============================
    // UPLOAD FILE TO DATAVERSE
    // ===============================
    uploadFileToDataverse: function (entityName, recordId, fieldName, fileName, buffer) {

        return new Promise(function (resolve, reject) {

            var clientUrl = Xrm.Utility.getGlobalContext().getClientUrl();
            var uploadUrl = clientUrl + "/api/data/v9.2/" + entityName + "s(" + recordId + ")/" + fieldName;
            var byteArray = new Uint8Array(buffer);

            fetch(uploadUrl, {
                method: "PATCH",
                headers: {
                    "Content-Type": "application/octet-stream",
                    "x-ms-file-name": encodeURIComponent(fileName),
                    "OData-MaxVersion": "4.0",
                    "OData-Version": "4.0",
                    "Accept": "application/json"
                },
                body: byteArray
            }).then(function (response) {
                if (response.ok || response.status === 204) {
                    resolve();
                } else {
                    response.json().then(function (err) {
                        reject(new Error(err.error ? err.error.message : "File upload failed: " + response.status));
                    }).catch(function () {
                        reject(new Error("File upload failed with status " + response.status));
                    });
                }
            }).catch(function (err) {
                reject(err);
            });
        });
    },

    // ===============================
    // DEACTIVATE RECORD
    // ===============================
    deactivateRecord: function (entityName, recordId) {

        return new Promise(function (resolve, reject) {

            var clientUrl = Xrm.Utility.getGlobalContext().getClientUrl();

            fetch(clientUrl + "/api/data/v9.2/" + entityName + "s(" + recordId + ")", {
                method: "PATCH",
                headers: {
                    "Content-Type": "application/json",
                    "OData-MaxVersion": "4.0",
                    "OData-Version": "4.0",
                    "Accept": "application/json"
                },
                body: JSON.stringify({
                    statecode: 0,
                    statuscode: 755700013
                })
            }).then(function (response) {
                if (response.ok || response.status === 204) {
                    resolve();
                } else {
                    response.json().then(function (err) {
                        reject(new Error(err.error ? err.error.message : "Deactivation failed: " + response.status));
                    }).catch(function () {
                        reject(new Error("Deactivation failed with status " + response.status));
                    });
                }
            }).catch(function (err) {
                reject(err);
            });
        });
    },

    // ===============================
    // ACTIVATE CANCELLATION BPF
    // ===============================
    activateCancellationBpf: function (entityName, formContext) {
        var processUniqueName = "bcc_cancellationbpf";
        var query = "?$select=workflowid,uniquename&$filter=uniquename eq '" +
            processUniqueName + "' and category eq 4 and statecode eq 1";

        if (!formContext || !formContext.data || !formContext.data.process ||
            typeof formContext.data.process.setActiveProcess !== "function") {
            return Promise.reject(new Error(
                "The record form does not expose the BPF process control."
            ));
        }

        return Xrm.WebApi.retrieveMultipleRecords("workflow", query).then(function (result) {
            var processes = result.entities;
            if (processes.length !== 1) {
                throw new Error(
                    "Expected one active Cancellation BPF (" + processUniqueName +
                    "), but found " + processes.length + "."
                );
            }

            return new Promise(function (resolve, reject) {
                formContext.data.process.setActiveProcess(
                    processes[0].workflowid.toUpperCase(),
                    function (status) {
                        if (status === "success") {
                            resolve();
                        } else {
                            reject(new Error(
                                "Could not switch to the Cancellation BPF (" + status + "). " +
                                "Confirm the process is enabled for " + entityName +
                                " and available to this user."
                            ));
                        }
                    }
                );
            });
        });
    },

    // ===============================
    // REMOVE MODAL IF EXISTS
    // ===============================
    removeModal: function () {
        var targetDoc = CancelOnDeactivate.getTargetDoc();
        var existing = targetDoc.getElementById("cancelModalOverlay");
        if (existing) existing.parentNode.removeChild(existing);
    },

    // ===============================
    // CANCEL POPUP (inline modal)
    // ===============================
    showCancelPopup: function (formContext, entityName, recordId) {

        recordId = recordId.replace(/[{}]/g, "");

        CancelOnDeactivate.removeModal();

        var targetDoc = CancelOnDeactivate.getTargetDoc();

        // ===============================
        // CREATE OVERLAY
        // ===============================
        var overlay = targetDoc.createElement("div");
        overlay.id = "cancelModalOverlay";
        overlay.style.cssText = [
            "position:fixed",
            "top:0",
            "left:0",
            "width:100%",
            "height:100%",
            "background:rgba(0,0,0,0.5)",
            "z-index:99999",
            "display:flex",
            "justify-content:center",
            "align-items:center",
            "font-family:Segoe UI, sans-serif"
        ].join(";");

        // ===============================
        // CREATE CARD
        // ===============================
        var card = targetDoc.createElement("div");
        card.style.cssText = [
            "background:#ffffff",
            "width:440px",
            "border-radius:8px",
            "box-shadow:0 8px 30px rgba(0,0,0,0.2)",
            "overflow:hidden"
        ].join(";");

        // ===============================
        // CARD HEADER
        // ===============================
        var header = targetDoc.createElement("div");
        header.style.cssText = [
            "background:#0078d4",
            "color:#ffffff",
            "padding:16px 20px",
            "display:flex",
            "justify-content:space-between",
            "align-items:center"
        ].join(";");

        var title = targetDoc.createElement("span");
        title.innerText = "Cancellation Details";
        title.style.cssText = "font-size:16px;font-weight:600;";

        var closeBtn = targetDoc.createElement("span");
        closeBtn.innerText = "✕";
        closeBtn.style.cssText = [
            "cursor:pointer",
            "font-size:16px",
            "color:#ffffff",
            "line-height:1"
        ].join(";");
        closeBtn.onclick = function () {
            CancelOnDeactivate.removeModal();
        };

        header.appendChild(title);
        header.appendChild(closeBtn);

        // ===============================
        // CARD BODY
        // ===============================
        var body = targetDoc.createElement("div");
        body.style.cssText = "padding:24px 20px;";

        // Cancellation Type Label — MANDATORY
        var cancellationTypeLabel = targetDoc.createElement("label");
        cancellationTypeLabel.innerHTML = "Cancellation Type <span style='color:#d13438;'>*</span>";
        cancellationTypeLabel.style.cssText = "font-weight:600;font-size:13px;display:block;margin-bottom:6px;color:#323130;";

        // Cancellation Type Picklist
        var cancellationTypeInput = targetDoc.createElement("select");
        cancellationTypeInput.id = "cancellationTypeInput";
        cancellationTypeInput.style.cssText = [
            "width:100%",
            "height:34px",
            "font-size:13px",
            "font-family:Segoe UI,sans-serif",
            "border:1px solid #8a8886",
            "border-radius:4px",
            "padding:4px 8px",
            "box-sizing:border-box",
            "margin-bottom:4px",
            "outline:none",
            "background:#ffffff"
        ].join(";");

        var cancellationTypePlaceholder = targetDoc.createElement("option");
        cancellationTypePlaceholder.value = "";
        cancellationTypePlaceholder.innerText = "Select cancellation type...";
        cancellationTypeInput.appendChild(cancellationTypePlaceholder);

        [
            { value: "755700000", text: "Resigned" },
            { value: "755700001", text: "Terminated" }
        ].forEach(function (option) {
            var optionElement = targetDoc.createElement("option");
            optionElement.value = option.value;
            optionElement.innerText = option.text;
            cancellationTypeInput.appendChild(optionElement);
        });

        var cancellationTypeError = targetDoc.createElement("div");
        cancellationTypeError.innerText = "Cancellation Type is required.";
        cancellationTypeError.style.cssText = "color:#d13438;font-size:11px;margin-bottom:12px;display:none;";

        // Cancel Reason Label — MANDATORY (shows red asterisk)
        var reasonLabel = targetDoc.createElement("label");
        reasonLabel.innerHTML = "Cancel Reason <span style='color:#d13438;'>*</span>";
        reasonLabel.style.cssText = "font-weight:600;font-size:13px;display:block;margin-bottom:6px;color:#323130;";

        // Cancel Reason Textarea
        var reasonInput = targetDoc.createElement("textarea");
        reasonInput.id = "cancelReasonInput";
        reasonInput.placeholder = "Enter cancel reason...";
        reasonInput.style.cssText = [
            "width:100%",
            "height:90px",
            "font-size:13px",
            "font-family:Segoe UI,sans-serif",
            "border:1px solid #8a8886",
            "border-radius:4px",
            "padding:8px",
            "box-sizing:border-box",
            "resize:none",
            "margin-bottom:4px",
            "outline:none"
        ].join(";");

        // Reason error message
        var reasonError = targetDoc.createElement("div");
        reasonError.innerText = "Cancel Reason is required.";
        reasonError.style.cssText = "color:#d13438;font-size:11px;margin-bottom:12px;display:none;";

        // File Label — NOT MANDATORY (no asterisk)
        var fileLabel = targetDoc.createElement("label");
        fileLabel.innerText = "Cancel Document";
        fileLabel.style.cssText = "font-weight:600;font-size:13px;display:block;margin-bottom:6px;color:#323130;";

        // File Input wrapper
        var fileWrapper = targetDoc.createElement("div");
        fileWrapper.style.cssText = [
            "border:1px solid #8a8886",
            "border-radius:4px",
            "padding:8px 10px",
            "margin-bottom:8px",
            "display:flex",
            "align-items:center",
            "gap:10px",
            "background:#faf9f8"
        ].join(";");

        // ✅ FIX: file input with required=false to prevent browser native validation
        var fileInput = targetDoc.createElement("input");
        fileInput.type = "file";
        fileInput.id = "cancelFileInput";
        fileInput.required = false;
        fileInput.removeAttribute("required");
        fileInput.style.cssText = "font-size:12px;font-family:Segoe UI,sans-serif;flex:1;border:none;background:transparent;outline:none;";

        fileWrapper.appendChild(fileInput);

        // File name display
        var fileNameDisplay = targetDoc.createElement("div");
        fileNameDisplay.style.cssText = "font-size:11px;color:#605e5c;margin-bottom:16px;min-height:16px;";

        fileInput.onchange = function () {
            if (fileInput.files && fileInput.files[0]) {
                fileNameDisplay.innerText = "Selected: " + fileInput.files[0].name;
            } else {
                fileNameDisplay.innerText = "";
            }
        };

        // Second cancel document label — NOT MANDATORY
        var cancellationFormLabel = targetDoc.createElement("label");
        cancellationFormLabel.innerText = "Cancel Document 1";
        cancellationFormLabel.style.cssText = "font-weight:600;font-size:13px;display:block;margin-bottom:6px;color:#323130;";

        // Second cancel document input
        var cancellationFormWrapper = targetDoc.createElement("div");
        cancellationFormWrapper.style.cssText = [
            "border:1px solid #8a8886",
            "border-radius:4px",
            "padding:8px 10px",
            "margin-bottom:8px",
            "display:flex",
            "align-items:center",
            "gap:10px",
            "background:#faf9f8"
        ].join(";");

        var cancellationFormInput = targetDoc.createElement("input");
        cancellationFormInput.type = "file";
        cancellationFormInput.id = "cancellationFormInput";
        cancellationFormInput.required = false;
        cancellationFormInput.removeAttribute("required");
        cancellationFormInput.style.cssText = "font-size:12px;font-family:Segoe UI,sans-serif;flex:1;border:none;background:transparent;outline:none;";
        cancellationFormWrapper.appendChild(cancellationFormInput);

        var cancellationFormNameDisplay = targetDoc.createElement("div");
        cancellationFormNameDisplay.style.cssText = "font-size:11px;color:#605e5c;margin-bottom:16px;min-height:16px;";

        cancellationFormInput.onchange = function () {
            if (cancellationFormInput.files && cancellationFormInput.files[0]) {
                cancellationFormNameDisplay.innerText = "Selected: " + cancellationFormInput.files[0].name;
            } else {
                cancellationFormNameDisplay.innerText = "";
            }
        };

        // ===============================
        // CARD FOOTER
        // ===============================
        var footer = targetDoc.createElement("div");
        footer.style.cssText = [
            "padding:12px 20px",
            "background:#f3f2f1",
            "display:flex",
            "justify-content:flex-end",
            "gap:10px",
            "border-top:1px solid #edebe9"
        ].join(";");

        // ✅ FIX: type="button" prevents browser native form validation
        var cancelBtn = targetDoc.createElement("button");
        cancelBtn.type = "button";
        cancelBtn.innerText = "Cancel";
        cancelBtn.style.cssText = [
            "padding:8px 20px",
            "font-size:13px",
            "font-family:Segoe UI,sans-serif",
            "border:1px solid #8a8886",
            "border-radius:4px",
            "background:#ffffff",
            "color:#323130",
            "cursor:pointer"
        ].join(";");
        cancelBtn.onclick = function () {
            CancelOnDeactivate.removeModal();
        };

        // ✅ FIX: type="button" prevents browser native form validation
        var submitBtn = targetDoc.createElement("button");
        submitBtn.type = "button";
        submitBtn.innerText = "Submit";
        submitBtn.style.cssText = [
            "padding:8px 20px",
            "font-size:13px",
            "font-family:Segoe UI,sans-serif",
            "border:none",
            "border-radius:4px",
            "background:#0078d4",
            "color:#ffffff",
            "cursor:pointer"
        ].join(";");

        submitBtn.onclick = function () {

            var reason = reasonInput.value.trim();
            var cancellationType = cancellationTypeInput.value;
            var file = fileInput.files[0]; // optional
            var cancellationFormFile = cancellationFormInput.files[0]; // optional

            if (!cancellationType) {
                cancellationTypeInput.style.border = "1px solid #d13438";
                cancellationTypeError.style.display = "block";
                cancellationTypeInput.focus();
                return;
            } else {
                cancellationTypeInput.style.border = "1px solid #8a8886";
                cancellationTypeError.style.display = "none";
            }

            // Only validate Cancel Reason — file is optional
            if (!reason) {
                reasonInput.style.border = "1px solid #d13438";
                reasonError.style.display = "block";
                reasonInput.focus();
                return;
            } else {
                reasonInput.style.border = "1px solid #8a8886";
                reasonError.style.display = "none";
            }

            // Disable submit to prevent double click
            submitBtn.disabled = true;
            submitBtn.innerText = "Processing...";
            submitBtn.style.background = "#106ebe";

            // ===============================
            // STEP 1: Save Cancel Reason
            // ===============================
            Xrm.WebApi.updateRecord(entityName, recordId, {
                bcc_cancelreason: reason,
                bcc_cancellationtype: parseInt(cancellationType, 10)
            }).then(function () {

                // ===============================
                // STEP 2: Upload cancel document only if selected
                // ===============================
                if (file) {
                    return new Promise(function (resolve, reject) {
                        var reader = new FileReader();
                        reader.onload = function () {
                            CancelOnDeactivate.uploadFileToDataverse(
                                entityName,
                                recordId,
                                "bcc_canceldocument",
                                file.name,
                                reader.result
                            ).then(resolve).catch(reject);
                        };
                        reader.readAsArrayBuffer(file);
                    });
                } else {
                    // No file selected — skip upload
                    return Promise.resolve();
                }

            }).then(function () {

                if (file) {
                    return Xrm.WebApi.updateRecord(entityName, recordId, {
                        bcc_canceldocumentuploaddate: new Date().toISOString()
                    });
                } else {
                    return Promise.resolve();
                }

            }).then(function () {

                // ===============================
                // STEP 3: Upload second cancel document only if selected
                // ===============================
                if (cancellationFormFile) {
                    return new Promise(function (resolve, reject) {
                        var reader = new FileReader();
                        reader.onload = function () {
                            CancelOnDeactivate.uploadFileToDataverse(
                                entityName,
                                recordId,
                                "bcc_canceldocument1",
                                cancellationFormFile.name,
                                reader.result
                            ).then(resolve).catch(reject);
                        };
                        reader.readAsArrayBuffer(cancellationFormFile);
                    });
                }
                return Promise.resolve();

            }).then(function () {

                // ===============================
                // STEP 4: Deactivate the record
                // ===============================
                return CancelOnDeactivate.deactivateRecord(entityName, recordId);

            }).then(function () {

                // ===============================
                // STEP 5: Switch the form directly to the Cancellation BPF.
                // ===============================
                return CancelOnDeactivate.activateCancellationBpf(entityName, formContext)
                    .catch(function (err) {
                        throw new Error(
                            "Record was deactivated, but the Cancellation BPF could not be activated: " +
                            (err.message || JSON.stringify(err))
                        );
                    });

            }).then(function () {

                CancelOnDeactivate.removeModal();

                // ===============================
                // STEP 6: Success + Navigate
                // ===============================
                Xrm.Navigation.openAlertDialog({
                    title: "Success",
                    text: "Record has been cancelled, deactivated, and moved to the Cancellation BPF."
                }).then(function () {

                    if (formContext) {
                        var currentRecordId = formContext.data.entity.getId();
                        var currentEntityName = formContext.data.entity.getEntityName();
                        Xrm.Navigation.navigateTo({
                            pageType: "entityrecord",
                            entityName: currentEntityName,
                            entityId: currentRecordId
                        }, {
                            target: 1
                        });
                    } else {
                        Xrm.Navigation.navigateTo({
                            pageType: "entitylist",
                            entityName: entityName
                        }, {
                            target: 1
                        });
                    }
                });

            }).catch(function (err) {
                CancelOnDeactivate.removeModal();
                Xrm.Navigation.openAlertDialog({
                    title: "Error",
                    text: "Operation failed: " + (err.message || JSON.stringify(err))
                });
            });
        };

        // ===============================
        // ASSEMBLE MODAL
        // ===============================
        body.appendChild(cancellationTypeLabel);
        body.appendChild(cancellationTypeInput);
        body.appendChild(cancellationTypeError);
        body.appendChild(reasonLabel);
        body.appendChild(reasonInput);
        body.appendChild(reasonError);
        body.appendChild(fileLabel);
        body.appendChild(fileWrapper);
        body.appendChild(fileNameDisplay);
        body.appendChild(cancellationFormLabel);
        body.appendChild(cancellationFormWrapper);
        body.appendChild(cancellationFormNameDisplay);

        footer.appendChild(cancelBtn);
        footer.appendChild(submitBtn);

        card.appendChild(header);
        card.appendChild(body);
        card.appendChild(footer);

        overlay.appendChild(card);
        targetDoc.body.appendChild(overlay);

        setTimeout(function () {
            reasonInput.focus();
        }, 100);
    }
};