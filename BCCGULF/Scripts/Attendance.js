if (typeof (Attendance) === "undefined") {
    var Attendance = {};
}

if (typeof (Attendance.Form) === "undefined") {
    Attendance.Form = {};
}

if (typeof (Attendance.Ribbon) === "undefined") {
    Attendance.Ribbon = {};
}

var AttendanceEntityName = "bcc_attendance";
var AttendanceStatusField = "statuscode";

var AttendanceStatus = {
    Draft: 1,
    PendingApproval: 755700001,
    Approved: 2,
    Rejected: 755700002
};

Attendance.Ribbon = {

    executeAttendanceAction: function (action, selectedControl, selectedRecordIds) {
        try {
            if (!selectedRecordIds || selectedRecordIds.length === 0) {
                Attendance.Ribbon.showErrorAlert("Please select at least one record.");
                return;
            }

            var progressMessages = { sendForApproval: "Sending for approval...", approve: "Approving...", reject: "Rejecting..." };
            var successMessages = { sendForApproval: "Attendance record(s) sent for approval successfully.", approve: "Attendance record(s) approved successfully.", reject: "Attendance record(s) rejected successfully." };
            var errorMessages = { sendForApproval: "Failed to send attendance for approval. Please try again.", approve: "Failed to approve attendance records. Please try again.", reject: "Failed to reject attendance records. Please try again." };

            Attendance.Ribbon.showProgress(progressMessages[action]);

            var userId = Xrm.Utility.getGlobalContext().getUserId().replace(/[{}]/g, "");

            var request = {
                action: action,
                selectedRecords: selectedRecordIds.join(","),
                userId: { "@odata.type": "Microsoft.Dynamics.CRM.systemuser", systemuserid: userId },
                getMetadata: function () {
                    return {
                        boundParameter: null,
                        parameterTypes: {
                            action: { typeName: "Edm.String", structuralProperty: 1 },
                            selectedRecords: { typeName: "Edm.String", structuralProperty: 1 },
                            userId: { typeName: "mscrm.systemuser", structuralProperty: 5 }
                        },
                        operationType: 0,
                        operationName: "bcc_AttendanceAction"
                    };
                }
            };

            Xrm.WebApi.execute(request).then(
                function (response) {
                    if (response.ok) { return response.json(); }
                }
            ).then(
                function (responseBody) {
                    Attendance.Ribbon.hideProgress();
                    var result = responseBody && responseBody["result"];
                    if (result && result.toLowerCase().indexOf("error") !== -1) {
                        Attendance.Ribbon.showErrorAlert(result);
                    } else {
                        Attendance.Ribbon.showSuccessAlert(successMessages[action]);
                        selectedControl.refresh();
                    }
                }
            ).catch(
                function (error) {
                    Attendance.Ribbon.hideProgress();
                    console.error("Error in executeAttendanceAction (" + action + "): " + error.message);
                    Attendance.Ribbon.showErrorAlert(errorMessages[action]);
                }
            );
        } catch (error) {
            Attendance.Ribbon.hideProgress();
            console.error("Error in executeAttendanceAction (" + action + "): " + error.message);
            Attendance.Ribbon.showErrorAlert(error.message);
        }
    },

    onClickSendForApproval: function (selectedControl, selectedRecordIds) {
        Attendance.Ribbon.showCommentDialog("sendForApproval", selectedControl, selectedRecordIds);
    },

    onClickApprove: function (selectedControl, selectedRecordIds) {
        Attendance.Ribbon.executeAttendanceAction("approve", selectedControl, selectedRecordIds);
    },

    onClickReject: function (selectedControl, selectedRecordIds) {
        Attendance.Ribbon.showCommentDialog("reject", selectedControl, selectedRecordIds);
    },

    showCommentDialog: function (action, selectedControl, selectedRecordIds) {
        try {
            if (!selectedRecordIds || selectedRecordIds.length === 0) {
                Attendance.Ribbon.showErrorAlert("Please select at least one record.");
                return;
            }

            var userId = Xrm.Utility.getGlobalContext().getUserId().replace(/[{}]/g, "");
            var dialogData = JSON.stringify({
                action: action,
                selectedRecords: selectedRecordIds.join(","),
                userId: userId
            });

            Xrm.Navigation.navigateTo(
                {
                    pageType: "webresource",
                    webresourceName: "bcc_AttendanceCommentDialog",
                    data: dialogData
                },
                {
                    target: 2,
                    width: { value: 480, unit: "px" },
                    height: { value: 300, unit: "px" }
                }
            ).then(
                function () {
                    selectedControl.refresh();
                }
            ).catch(
                function (error) {
                    console.error("Error opening comment dialog: " + error.message);
                    Attendance.Ribbon.showErrorAlert("Failed to open comment dialog. Please try again.");
                }
            );
        } catch (error) {
            console.error("Error in showCommentDialog: " + error.message);
            Attendance.Ribbon.showErrorAlert(error.message);
        }
    },

    /**
     * Enable rule: show Send For Approval only when all selected records are in Draft status.
     * Bind to EnableRule in the ribbon XML with SelectedControl parameter.
     */
    isSendForApprovalEnabled: function (selectedControl) {
        try {
            debugger;
            var selectedRows = selectedControl.getGrid().getSelectedRows();
            if (selectedRows.getLength() === 0) return false;

            var rows = [];
            selectedRows.forEach(function (row) { rows.push(row); });

            return rows.every(function (row) {
                var status = row.getData().getEntity().attributes.get(AttendanceStatusField);
                if (!status) return false;
                return status.getValue() === AttendanceStatus.Draft || status.getValue() === AttendanceStatus.Rejected;
            });
        } catch (error) {
            console.error("Error in isSendForApprovalEnabled: " + error.message);
            return false;
        }
    },

    /**
     * Enable rule: show Approve / Reject only when all selected records are Pending Approval.
     * Bind to EnableRule in the ribbon XML with SelectedControl parameter.
     */
    isPendingApprovalEnabled: function (selectedControl) {
        try {
            debugger;
            var selectedRows = selectedControl.getGrid().getSelectedRows();

            if (selectedRows.getLength() === 0) return false;

            var allPending = true;
            selectedRows.forEach(function (row) {
                var status = row.getData().getEntity().attributes.get(AttendanceStatusField);
                if (!status || status.getValue() !== AttendanceStatus.PendingApproval) {
                    allPending = false;
                }
            });

            return allPending;
        } catch (error) {
            console.error("Error in isPendingApprovalEnabled: " + error.message);
            return false;
        }
    },

    showSuccessAlert: function (message) {
        try {
            let alertStrings = { confirmButtonLabel: "Close", text: message };
            let alertOptions = { height: 120, width: 260 };

            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                function success() {},
                function (error) {
                    console.log(error.message);
                }
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
            let alertStrings = { confirmButtonLabel: "Close", text: "Error: " + errorMessage };
            let alertOptions = { height: 120, width: 260 };

            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                function success() {},
                function (error) {
                    console.log(error.message);
                }
            );
        } catch (error) {
            console.error("Error showing error alert: " + error.message);
        }
    }
};
