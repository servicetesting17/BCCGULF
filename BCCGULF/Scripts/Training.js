if (typeof (Training) === "undefined") {
    var Training = {};
}

if (typeof (Training.Form) === "undefined") {
    Training.Form = {};
}

if (typeof (Training.Ribbon) === "undefined") {
    Training.Ribbon = {};
}

var TrainingEntityName = "bcc_training";
var TrainingEntityCollectionName = "bcc_trainings";
var TrainingStatusField = "statecode";
var TrainingStatusReasonField = "statuscode";
var TrainingEmployeeSignatureField = "bcc_employeesignature";
var TrainingManagerSignatureField = "bcc_managersignature";

var TrainingStatus = {
    Active: 0,
    Inactive: 1
};

var TrainingStatusReason = {
    Active: 1,
    Inactive: 2
};

Training.Ribbon = {

    /* ── Main Grid ─────────────────────────────────────────────────────────── */

    onClickCloseTraining: async function (selectedControl, selectedRecordIds) {
        try {
            if (!selectedRecordIds || selectedRecordIds.length === 0) {
                Training.Ribbon.showErrorAlert("Please select at least one record.");
                return;
            }

            Training.Ribbon.showProgress("Validating training record(s)...");

            var validationErrors = [];

            for (var i = 0; i < selectedRecordIds.length; i++) {
                var recordId = selectedRecordIds[i].replace(/[{}]/g, "");
                try {
                    var record = await Xrm.WebApi.retrieveRecord(
                        TrainingEntityName,
                        recordId,
                        "?$select=bcc_name," + TrainingEmployeeSignatureField + "," + TrainingManagerSignatureField
                    );

                    var missingFields = [];
                    if (!record[TrainingEmployeeSignatureField]) {
                        missingFields.push("Employee Signature");
                    }
                    if (!record[TrainingManagerSignatureField]) {
                        missingFields.push("Manager Signature");
                    }

                    if (missingFields.length > 0) {
                        validationErrors.push(
                            "Record '" + (record["bcc_name"] || recordId) + "': " + missingFields.join(" and ") + " is required."
                        );
                    }
                } catch (fetchError) {
                    validationErrors.push("Failed to retrieve record " + recordId + ": " + fetchError.message);
                }
            }

            if (validationErrors.length > 0) {
                Training.Ribbon.hideProgress();
                Training.Ribbon.showErrorAlert(validationErrors.join("\n"));
                return;
            }

            Training.Ribbon.showProgress("Closing training record(s)...");

            var closePromises = selectedRecordIds.map(function (id) {
                var rid = id.replace(/[{}]/g, "");
                return Xrm.WebApi.updateRecord(TrainingEntityName, rid, {
                    "statecode": TrainingStatus.Inactive,
                    "statuscode": TrainingStatusReason.Inactive
                });
            });

            Promise.all(closePromises).then(
                function () {
                    Training.Ribbon.hideProgress();
                    Training.Ribbon.showSuccessAlert("Training record(s) closed successfully.");
                    selectedControl.refresh();
                }
            ).catch(
                function (error) {
                    Training.Ribbon.hideProgress();
                    console.error("Error closing training records: " + error.message);
                    Training.Ribbon.showErrorAlert("Failed to close training record(s). Please try again.");
                }
            );

        } catch (error) {
            Training.Ribbon.hideProgress();
            console.error("Error in onClickCloseTraining: " + error.message);
            Training.Ribbon.showErrorAlert(error.message);
        }
    },

    /**
     * Enable rule: show Close Training on the grid only when all selected records are Active.
     */
    isCloseTrainingEnabled: function (selectedControl) {
        try {
            var selectedRows = selectedControl.getGrid().getSelectedRows();
            if (selectedRows.getLength() === 0) return false;

            var rows = [];
            selectedRows.forEach(function (row) { rows.push(row); });

            return rows.every(function (row) {
                var status = row.getData().getEntity().attributes.get(TrainingStatusField);
                if (!status) return false;
                return status.getValue() === TrainingStatus.Active;
            });
        } catch (error) {
            console.error("Error in isCloseTrainingEnabled: " + error.message);
            return false;
        }
    },

    /* ── Form ──────────────────────────────────────────────────────────────── */

    onClickCloseTrainingForm: async function (formContext) {
        try {
            var recordId = formContext.data.entity.getId().replace(/[{}]/g, "");

            Training.Ribbon.showProgress("Validating training record...");

            var record = await Xrm.WebApi.retrieveRecord(
                TrainingEntityName,
                recordId,
                "?$select=" + TrainingEmployeeSignatureField + "," + TrainingManagerSignatureField
            );

            var missingFields = [];
            if (!record[TrainingEmployeeSignatureField]) {
                missingFields.push("Employee Signature");
            }
            if (!record[TrainingManagerSignatureField]) {
                missingFields.push("Manager Signature");
            }

            if (missingFields.length > 0) {
                Training.Ribbon.hideProgress();
                Training.Ribbon.showErrorAlert(missingFields.join(" and ") + " is required to close this training record.");
                return;
            }

            Training.Ribbon.showProgress("Closing training record...");

            Xrm.WebApi.updateRecord(TrainingEntityName, recordId, {
                "statecode": TrainingStatus.Inactive,
                "statuscode": TrainingStatusReason.Inactive
            }).then(
                function () {
                    Training.Ribbon.hideProgress();
                    Training.Ribbon.showSuccessAlert("Training record closed successfully.");
                    formContext.data.refresh(false);
                },
                function (error) {
                    Training.Ribbon.hideProgress();
                    console.error("Error closing training record: " + error.message);
                    Training.Ribbon.showErrorAlert("Failed to close training record. Please try again.");
                }
            );

        } catch (error) {
            Training.Ribbon.hideProgress();
            console.error("Error in onClickCloseTrainingForm: " + error.message);
            Training.Ribbon.showErrorAlert(error.message);
        }
    },

    /**
     * Enable rule: show Close Training on the form only when the record is Active.
     */
    isCloseTrainingFormEnabled: function (formContext) {
        try {
            var statusAttr = formContext.getAttribute(TrainingStatusField);
            if (!statusAttr) return false;
            return statusAttr.getValue() === TrainingStatus.Active;
        } catch (error) {
            console.error("Error in isCloseTrainingFormEnabled: " + error.message);
            return false;
        }
    },

    /* ── Helpers ───────────────────────────────────────────────────────────── */

    showSuccessAlert: function (message) {
        try {
            var alertStrings = { confirmButtonLabel: "Close", text: message };
            var alertOptions = { height: 120, width: 260 };
            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                function success() {},
                function (error) { console.log(error.message); }
            );
        } catch (error) {
            console.error("Error showing success alert: " + error.message);
        }
    },

    showProgress: function (message) {
        try {
            Xrm.Utility.showProgressIndicator(message || "Please wait...");
        } catch (error) {
            console.error("Error showing progress: " + error.message);
        }
    },

    hideProgress: function () {
        try {
            Xrm.Utility.closeProgressIndicator();
        } catch (error) {
            console.error("Error hiding progress: " + error.message);
        }
    },

    showErrorAlert: function (errorMessage) {
        try {
            var alertStrings = { confirmButtonLabel: "Close", text: "Error: " + errorMessage };
            var alertOptions = { height: 120, width: 260 };
            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                function success() {},
                function (error) { console.log(error.message); }
            );
        } catch (error) {
            console.error("Error showing error alert: " + error.message);
        }
    }
};
