function approveSelectedRecords(selectedItems, selectedControl) {
    if (!selectedItems || selectedItems.length === 0) {
        Xrm.Navigation.openAlertDialog({ text: "Please select at least one record to approve." });
        return;
    }

    var updatePromises = [];

    selectedItems.forEach(function (item) {
        var id = item.Id.replace(/[{}]/g, "");
        var entityName = item.TypeName;
        var update = {};
        update.msdyn_entrystatus = 192350002; // Approved

        updatePromises.push(Xrm.WebApi.updateRecord(entityName, id, update));
    });

    Promise.all(updatePromises)
        .then(function () {
            refreshMainEditableGrid(selectedControl);
            Xrm.Navigation.openAlertDialog({ text: "Selected records approved successfully." });
        })
        .catch(function (error) {
            console.error("Error approving records:", error.message);
            Xrm.Navigation.openAlertDialog({ text: "Error approving records: " + error.message });
        });
}


async function rejectSelectedRecords(selectedItems, selectedControl) {
    try {
        if (!selectedItems || selectedItems.length === 0) {
            Xrm.Utility.alertDialog("Please select at least one record.");
            return;
        }

        // Collect GUIDs
        var recordIds = selectedItems.map(function (item) {
            return item.Id.replace(/[{}]/g, "");
        }).join(",");

        // Open popup (HTML Web Resource)
        var pageInput = {
            pageType: "webresource",
            webresourceName: "bcc_rejectreasonpage",
            data: recordIds
        };

        var navigationOptions = {
            target: 2,
            width: 450,
            height: 350,
            position: 1,
            title: "Reject Reason"
        };

        // WAIT until popup is closed
        Xrm.Navigation.navigateTo(pageInput, navigationOptions).then(
            function (returnValue) {

                console.log("Popup closed:", returnValue);

                // 🔥 Refresh grid ONLY after OK/Cancel pressed
                refreshMainEditableGrid(selectedControl);
            },
            function (error) {
                console.error(error);
            }
        );

    } catch (e) {
        console.error(e);
    }
}




// ✅ This refresh method works for main editable grid (view)
function refreshMainEditableGrid(selectedControl) {
    try {
        // If control has refresh method
        if (selectedControl && typeof selectedControl.refresh === "function") {
            selectedControl.refresh();
            console.log("Main grid refreshed via selectedControl.refresh()");
            return;
        }

        // If it has a grid object
        if (selectedControl && selectedControl.getGrid) {
            selectedControl.getGrid().refresh();
            console.log("Main grid refreshed via getGrid()");
            return;
        }

        // ✅ Fallback for main editable grid
        var gridControl = Xrm.Page && Xrm.Page.getControl
            ? Xrm.Page.getControl("crmGrid") || Xrm.Page.getControl("HomePageGrid")
            : null;

        if (gridControl && gridControl.refresh) {
            gridControl.refresh();
            console.log("Main editable grid refreshed via crmGrid/HomePageGrid");
            return;
        }

        // Last fallback: reload page (rarely needed)
        console.warn("Reloading page as last resort...");
        location.reload();

    } catch (e) {
        console.error("Grid refresh error:", e);
    }
}

function showApproveButtonByView(selectedControl) {
    const allowedViewId = "cbcafdec-ebba-f011-bbd3-6045bd15153f"; 

    try {
        var viewSelector = selectedControl.getViewSelector();
        var currentView = viewSelector ? viewSelector.getCurrentView() : null;
        if (!currentView) return false;

        var currentViewId = currentView.id.replace(/[{}]/g, "");
        return currentViewId.toLowerCase() === allowedViewId.toLowerCase();
    } catch (e) {
        console.error(e);
        return false;
    }
}

async function clikOnSendForApproval(selectedItems) {
    if (!selectedItems || selectedItems.length === 0) {
        Xrm.Navigation.openAlertDialog({ text: "Please select at least one record to approve." });
        return;
    }

    try {
        const firstItem = selectedItems[0];
        const entityName = firstItem.TypeName;
        const firstId = firstItem.Id.replace(/[{}]/g, "");

        // Step 1️⃣ — Retrieve approver from the first record
        const record = await Xrm.WebApi.retrieveRecord(entityName, firstId, "?$select=_bcc_approver_value");
        const approverId = record["_bcc_approver_value"]; // <-- replace with your actual field schema name

        if (!approverId) {
            Xrm.Navigation.openAlertDialog({ text: "Approver not found on the record." });
            return;
        }

        // Step 2️⃣ — Update all selected records to "Pending for Approval"
        const updatePromises = selectedItems.map(item => {
            const id = item.Id.replace(/[{}]/g, "");
            const update = { msdyn_entrystatus: 192350004 }; // Pending for Approval
            return Xrm.WebApi.updateRecord(entityName, id, update);
        });

        await Promise.all(updatePromises);
        console.log(`${selectedItems.length} records updated to Pending for Approval.`);

        // Step 3️⃣ — Send a single email to the approver for all items
        await sendApprovalEmail(approverId);

        Xrm.Navigation.openAlertDialog({ text: "Status updated and email sent to approver." });

    } catch (error) {
        console.error("Error in Send for Approval:", error.message);
        Xrm.Navigation.openAlertDialog({ text: "Error: " + error.message });
    }
}

function enableDisableRuleSendForApproval(selectedControl){
  const allowedViewId = "597f9f08-eab6-f011-bbd2-002248d37ceb"; 

    try {
        var viewSelector = selectedControl.getViewSelector();
        var currentView = viewSelector ? viewSelector.getCurrentView() : null;
        if (!currentView) return false;

        var currentViewId = currentView.id.replace(/[{}]/g, "");
        return currentViewId.toLowerCase() === allowedViewId.toLowerCase();
    } catch (e) {
        console.error(e);
        return false;
    }
}


async function sendApprovalEmail(approverId) {
    try {
        const orgUrl = Xrm.Utility.getGlobalContext().getClientUrl();
        const userId = "f0d0bb90-3fb1-f011-bbd3-6045bd15021a";
        approverId = approverId.replace(/[{}]/g, "");

        // Create the email record
        const email = {
            subject: "Approval Required: Time Entries",
            description: `
                <p>Hello,</p>
                <p>Please review and approve the submitted time entries.</p>
                <p><a href='https://bccgulf.crm15.dynamics.com/main.aspx?appid=ba98fb8b-88b5-f011-bbd2-6045bd6aa790&ribbondebug=true&pagetype=entitylist&etn=msdyn_timeentry&viewid=cbcafdec-ebba-f011-bbd3-6045bd15153f&viewType=1039'>Approval Link</a></p>
                <p>Thank you</p> 
                <p>Regards,<br/>BCCGulf Consultant</p>
            `,
            directioncode: true,
            email_activity_parties: [
                { "partyid_systemuser@odata.bind": `/systemusers(${userId})`, participationtypemask: 1 }, // From
                { "partyid_systemuser@odata.bind": `/systemusers(${approverId})`, participationtypemask: 2 } // To
            ]
        };

        const emailResult = await Xrm.WebApi.createRecord("email", email);
        console.log("✅ Email created:", emailResult.id);

        // Correctly bound SendEmail request
        const sendEmailRequest = {
            entity: { entityType: "email", id: emailResult.id }, // ❗ bound to entity
            IssueSend: true,
            getMetadata: function () {
                return {
                    boundParameter: "entity",
                    operationName: "SendEmail",
                    operationType: 0,
                    parameterTypes: {
                        entity: { typeName: "Microsoft.Dynamics.CRM.email", structuralProperty: 5 },
                        IssueSend: { typeName: "Edm.Boolean", structuralProperty: 1 }
                    }
                };
            }
        };

        await Xrm.WebApi.online.execute(sendEmailRequest);
        console.log("✅ Email sent successfully.");
        Xrm.Navigation.openAlertDialog({ text: "Approval email sent successfully." });

    } catch (error) {
        console.error("❌ Error sending email:", error);
        Xrm.Navigation.openAlertDialog({ text: "Error sending email: " + error.message });
    }
}

function onClickProceesPayroll(primaryControl) {
    var entityName = "";
    try {
        if (primaryControl && primaryControl.getEntityName) {
            entityName = primaryControl.getEntityName();                  // grid context
        } else if (primaryControl && primaryControl.data && primaryControl.data.entity) {
            entityName = primaryControl.data.entity.getEntityName();      // form context
        }
    } catch (e) {
        console.warn("onClickProcessPayroll: could not read entity name", e);
    }

    var data = entityName;

    Xrm.Navigation.navigateTo(
        {
            pageType: "webresource",
            webresourceName: "bcc_PayrollMonthDialog",
            data: data
        },
        {
            target: 2,
            width:  { value: 460, unit: "px" },
            height: { value: 360, unit: "px" },
            title:  "Process Payroll"
        }
    ).then(function () {
        if (primaryControl && typeof primaryControl.refresh === "function") {
            primaryControl.refresh();
        }
    });
}

async function onClickResume(formContext) {
    var confirmResult = await Xrm.Navigation.openConfirmDialog({
        title: "Resume from Approved Leave",
        text: "Are you sure you want to resume this resource from approved leave?",
        confirmButtonLabel: "Yes, Resume",
        cancelButtonLabel: "Cancel"
    });

    if (!confirmResult.confirmed) {
        return;
    }

    var workingStatusAttribute = formContext.getAttribute("bcc_workingstatus");
    if (!workingStatusAttribute) {
        await Xrm.Navigation.openAlertDialog({
            text: "The Working Status field is not available on this form."
        });
        return;
    }

    try {
        workingStatusAttribute.setValue(755700000);
        await formContext.data.save();
    } catch (error) {
        console.error("Error resuming resource from approved leave:", error);
        await Xrm.Navigation.openAlertDialog({
            text: "Error resuming resource: " + error.message
        });
    }
}