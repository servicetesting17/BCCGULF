if (typeof (Evaluation) === "undefined") {
    var Evaluation = {};
}

if (typeof (Evaluation.Form) === "undefined") {
    Evaluation.Form = {};
}

if (typeof (Evaluation.Ribbon) === "undefined") {
    Evaluation.Ribbon = {};
}

var EvaluationEntityName = "bcc_evaluation";
var EvaluationStatusField = "statuscode";
var EvaluationProjectKey = "bcc_projectid";
var ProjectEntityName = "bcc_project";
var ProjectPMKey = "_bcc_pmsmanagerid_value";

var EvaluationStatus = {
    Active: 1,
    PendingApproval: 755700001,
    Approved: 2,
    Rejected: 755700002
};

var EscalationRating = {
    Poor: 755700000,
    Average: 755700001
};

Evaluation.Ribbon = {

    /* ── Main Grid ─────────────────────────────────────────────────────────── */

    executeEvaluationAction: function (action, selectedControl, selectedRecordIds) {
        try {
            if (!selectedRecordIds || selectedRecordIds.length === 0) {
                Evaluation.Ribbon.showErrorAlert("Please select at least one record.");
                return;
            }

            var progressMessages = { sendForApproval: "Sending for approval...", approve: "Approving...", reject: "Rejecting..." };
            var successMessages  = { sendForApproval: "Evaluation record(s) sent for approval successfully.", approve: "Evaluation record(s) approved successfully.", reject: "Evaluation record(s) rejected successfully." };
            var errorMessages    = { sendForApproval: "Failed to send evaluation for approval. Please try again.", approve: "Failed to approve evaluation records. Please try again.", reject: "Failed to reject evaluation records. Please try again." };

            Evaluation.Ribbon.showProgress(progressMessages[action]);

            var userId = Xrm.Utility.getGlobalContext().getUserId().replace(/[{}]/g, "");

            var request = {
                action: action,
                selectedRecords: selectedRecordIds.join(","),
                userId: { "@odata.type": "Microsoft.Dynamics.CRM.systemuser", systemuserid: userId },
                getMetadata: function () {
                    return {
                        boundParameter: null,
                        parameterTypes: {
                            action:          { typeName: "Edm.String",       structuralProperty: 1 },
                            selectedRecords: { typeName: "Edm.String",       structuralProperty: 1 },
                            userId:          { typeName: "mscrm.systemuser", structuralProperty: 5 }
                        },
                        operationType: 0,
                        operationName: "bcc_EvaluationAction"
                    };
                }
            };

            Xrm.WebApi.execute(request).then(
                function (response) {
                    if (response.ok) { return response.json(); }
                }
            ).then(
                function (responseBody) {
                    Evaluation.Ribbon.hideProgress();
                    var result = responseBody && responseBody["result"];
                    if (result && result.toLowerCase().indexOf("error") !== -1) {
                        Evaluation.Ribbon.showErrorAlert(result);
                    } else {
                        Evaluation.Ribbon.showSuccessAlert(successMessages[action]);
                        selectedControl.refresh();
                    }
                }
            ).catch(
                function (error) {
                    Evaluation.Ribbon.hideProgress();
                    console.error("Error in executeEvaluationAction (" + action + "): " + error.message);
                    Evaluation.Ribbon.showErrorAlert(errorMessages[action]);
                }
            );
        } catch (error) {
            Evaluation.Ribbon.hideProgress();
            console.error("Error in executeEvaluationAction (" + action + "): " + error.message);
            Evaluation.Ribbon.showErrorAlert(error.message);
        }
    },

    onClickSendForApproval: function (selectedControl, selectedRecordIds) {
        Evaluation.Ribbon.executeEvaluationAction("sendForApproval", selectedControl, selectedRecordIds);
    },

    onClickApprove: function (selectedControl, selectedRecordIds) {
        Evaluation.Ribbon.executeEvaluationAction("approve", selectedControl, selectedRecordIds);
    },

    onClickReject: function (selectedControl, selectedRecordIds) {
        Evaluation.Ribbon.executeEvaluationAction("reject", selectedControl, selectedRecordIds);
    },

    /**
     * Enable rule: show Send For Approval only when all selected records are Active (Draft).
     */
    isSendForApprovalEnabled: function (selectedControl) {
        try {
            var selectedRows = selectedControl.getGrid().getSelectedRows();
            if (selectedRows.getLength() === 0) return false;

            var rows = [];
            selectedRows.forEach(function (row) { rows.push(row); });

            return rows.every(function (row) {
                var status = row.getData().getEntity().attributes.get(EvaluationStatusField);
                if (!status) return false;
                return status.getValue() === EvaluationStatus.Active || status.getValue() === EvaluationStatus.Rejected;
            });
        } catch (error) {
            console.error("Error in isSendForApprovalEnabled: " + error.message);
            return false;
        }
    },

    /**
     * Enable rule: show Approve / Reject only when all selected records are Pending Approval.
     */
    isPendingApprovalEnabled: function (selectedControl) {
        try {
            var selectedRows = selectedControl.getGrid().getSelectedRows();
            if (selectedRows.getLength() === 0) return false;

            var rows = [];
            selectedRows.forEach(function (row) { rows.push(row); });

            return rows.every(function (row) {
                var status = row.getData().getEntity().attributes.get(EvaluationStatusField);
                if (!status) return false;
                return status.getValue() === EvaluationStatus.PendingApproval;
            });
        } catch (error) {
            console.error("Error in isPendingApprovalEnabled: " + error.message);
            return false;
        }
    },

    /**
     * Enable rule: show Escalation button only when all selected records have rating Poor or Average.
     */
    isEscalationEnabled: function (selectedControl) {
        try {
            var selectedRows = selectedControl.getGrid().getSelectedRows();
            if (selectedRows.getLength() === 0) return false;

            var rows = [];
            selectedRows.forEach(function (row) { rows.push(row); });

            return rows.every(function (row) {
                var rating = row.getData().getEntity().attributes.get("bcc_rating");
                if (!rating) return false;
                var val = rating.getValue();
                return val === EscalationRating.Poor || val === EscalationRating.Average;
            });
        } catch (error) {
            console.error("Error in isEscalationEnabled: " + error.message);
            return false;
        }
    },

    onClickEscalation: function (selectedControl, selectedRecordIds) {
        try {
            if (!selectedRecordIds || selectedRecordIds.length === 0) {
                Evaluation.Ribbon.showErrorAlert("Please select at least one record.");
                return;
            }

            Evaluation.Ribbon.showProgress("Creating escalation record(s)...");

            var request = {
                selectedRecords: selectedRecordIds.join(","),
                getMetadata: function () {
                    return {
                        boundParameter: null,
                        parameterTypes: {
                            selectedRecords: { typeName: "Edm.String", structuralProperty: 1 }
                        },
                        operationType: 0,
                        operationName: "bcc_EscalationAction"
                    };
                }
            };

            Xrm.WebApi.execute(request).then(
                function (response) {
                    if (response.ok) { return response.json(); }
                }
            ).then(
                function (responseBody) {
                    Evaluation.Ribbon.hideProgress();
                    var result = responseBody && responseBody["result"];
                    if (result && result.toLowerCase().indexOf("error") !== -1) {
                        Evaluation.Ribbon.showErrorAlert(result);
                    } else {
                        Evaluation.Ribbon.showSuccessAlert(result || "Escalation record(s) created successfully.");
                        selectedControl.refresh();
                    }
                }
            ).catch(
                function (error) {
                    Evaluation.Ribbon.hideProgress();
                    console.error("Error in onClickEscalation: " + error.message);
                    Evaluation.Ribbon.showErrorAlert("Failed to create escalation record(s). Please try again.");
                }
            );

        } catch (error) {
            Evaluation.Ribbon.hideProgress();
            console.error("Error in onClickEscalation: " + error.message);
            Evaluation.Ribbon.showErrorAlert(error.message);
        }
    },

    /* ── Form (kept for the existing form button) ──────────────────────────── */

    onClickSendForApprovalForm: async function (formContext) {
        try {
            var projectLookup = formContext.getAttribute(EvaluationProjectKey).getValue();

            if (!projectLookup || projectLookup.length === 0) {
                Evaluation.Ribbon.showErrorAlert("Project is required.");
                return;
            }

            var projectId = projectLookup[0].id.replace("{", "").replace("}", "");

            var projectRecord = await Xrm.WebApi.retrieveRecord(
                ProjectEntityName,
                projectId,
                "?$select=" + ProjectPMKey
            );

            if (!projectRecord[ProjectPMKey]) {
                Evaluation.Ribbon.showErrorAlert("Project Manager is empty on the selected Project.");
                return;
            }

            formContext.getAttribute("statuscode").setValue(EvaluationStatus.PendingApproval);

            formContext.data.save().then(
                function success() {
                    Evaluation.Ribbon.showSuccessAlert("Evaluation sent for approval successfully.");
                },
                function (error) {
                    console.error("Error saving record: " + error.message);
                    Evaluation.Ribbon.showErrorAlert("Failed to send evaluation for approval. Please try again.");
                }
            );

        } catch (error) {
            console.error("Error in onClickSendForApprovalForm: " + error.message);
            Evaluation.Ribbon.showErrorAlert(error.message);
        }
    },

    /* ── Helpers ───────────────────────────────────────────────────────────── */

    showSuccessAlert: function (message) {
        try {
            let alertStrings = { confirmButtonLabel: 'Close', text: message };
            let alertOptions = { height: 120, width: 260 };
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
            let alertStrings = { confirmButtonLabel: 'Close', text: "Error: " + errorMessage };
            let alertOptions = { height: 120, width: 260 };
            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                function success() {},
                function (error) { console.log(error.message); }
            );
        } catch (error) {
            console.error("Error showing error alert: " + error.message);
        }
    }
};
