if (typeof (ManPowerRequest) === "undefined") {
    var ManPowerRequest = {};
}

if (typeof (ManPowerRequest.Form) === "undefined") {
    ManPowerRequest.Form = {};
}

if (typeof (ManPowerRequest.Ribbon) === "undefined") {
    ManPowerRequest.Ribbon = {};
}

ManPowerRequest.Ribbon = {
    onClickSendForApproval: function (formContext) {
        try {
           formContext.getAttribute("statuscode").setValue(755700001);
           formContext.data.entity.save();
        }
        catch (error) {
            console.error("ManPowerRequest.Ribbon.onClickSendForApproval - unexpected error: " + error.message);
        }
    },
    onClickApprove : function (formContext) {
        try {
            formContext.getAttribute("statuscode").setValue(755700002);
            formContext.data.entity.save();
        }
        catch (error) {
            console.error("ManPowerRequest.Ribbon.onClickApprove - unexpected error: " + error.message);
        }   
    },
    onClickReject: function (formContext) {
        try {
            formContext.getAttribute("statuscode").setValue(755700003);
            formContext.data.entity.save();
        }
        catch (error) {
            console.error("ManPowerRequest.Ribbon.onClickReject - unexpected error: " + error.message);
        }   
    }
};
// Form logic for ManPowerRequest
ManPowerRequest.Form = {
    config: {
        workforcePlanningField: "bcc_workforceplanningid",
        projectField: "bcc_project",
        bookableResourceField: "bcc_bookableresourceid",
        positionField: "bcc_designation",
        countofmanpower: "bcc_countofmanpower"
    },

    onFormLoad: function (executionContext) {
        try {
            var formContext = executionContext.getFormContext
                ? executionContext.getFormContext()
                : executionContext;

            var wpAttr = formContext.getAttribute(ManPowerRequest.Form.config.workforcePlanningField);
            if (wpAttr && !wpAttr.__bccgulfWorkforceHandlerAdded) {
                wpAttr.addOnChange(ManPowerRequest.Form.onChangeWorkforcePlanning);
                wpAttr.__bccgulfWorkforceHandlerAdded = true;
            }

            // run once on load to populate fields if present
            ManPowerRequest.Form.onChangeWorkforcePlanning(executionContext);
        }
        catch (error) {
            console.error("ManPowerRequest.Form.onFormLoad - unexpected error: " + error.message);
        }
    },

    onChangeWorkforcePlanning: async function (executionContext) {
        try {
            var formContext = executionContext.getFormContext
                ? executionContext.getFormContext()
                : executionContext;

            var wpAttr = formContext.getAttribute(ManPowerRequest.Form.config.workforcePlanningField);
           
            if (!wpAttr ) {
                return;
            }
             var projectAttr = formContext.getAttribute(ManPowerRequest.Form.config.projectField);
            var bookableAttr = formContext.getAttribute(ManPowerRequest.Form.config.bookableResourceField);
            var positionAttr = formContext.getAttribute(ManPowerRequest.Form.config.positionField);
            var countAttr = formContext.getAttribute(ManPowerRequest.Form.config.countofmanpower);

            var wpValue = wpAttr.getValue();
            if (!wpValue || wpValue.length === 0) {
                projectAttr.setValue(null);
                bookableAttr.setValue(null);
                positionAttr.setValue(null);
                return;
            }

            var wpId = wpValue[0].id.replace(/[{}]/g, "");

            var record = await Xrm.WebApi.retrieveRecord(
                "bcc_workforceplanning",
                wpId,
                "?$select=_bcc_projectid_value,_bcc_bookableresourceid_value,_bcc_positionid_value,bcc_resourcecount"
            );

            var projectValue = null;
            var bookableValue = null;

            if (record && record._bcc_projectid_value) {
                projectValue = [{
                    id: record._bcc_projectid_value,
                    name: record["_bcc_projectid_value@OData.Community.Display.V1.FormattedValue"] || "",
                    entityType: "bcc_project"
                }];
            }

            if (record && record._bcc_bookableresourceid_value) {
                bookableValue = [{
                    id: record._bcc_bookableresourceid_value,
                    name: record["_bcc_bookableresourceid_value@OData.Community.Display.V1.FormattedValue"] || "",
                    entityType: "bookableresource"
                }];
            }

            var positionValue = null;
            if (record && record._bcc_positionid_value) {
                positionValue = [{
                    id: record._bcc_positionid_value,
                    name: record["_bcc_positionid_value@OData.Community.Display.V1.FormattedValue"] || "",
                    entityType: "bcc_designation"
                }];
            }

            var countValue = null;
            if (record && record.bcc_resourcecount !== null && record.bcc_resourcecount !== undefined) {
                countValue = record.bcc_resourcecount;
            }

            projectAttr.setValue(projectValue);
            bookableAttr.setValue(bookableValue);
            positionAttr.setValue(positionValue);
            countAttr.setValue(countValue);
        }
        catch (error) {
            console.error("ManPowerRequest.Form.onChangeWorkforcePlanning - unexpected error: " + error.message);
        }
    }
};