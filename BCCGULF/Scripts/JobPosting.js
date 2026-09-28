if (typeof (JobPosting) === "undefined") {
    var JobPosting = {};
}

if (typeof (JobPosting.Form) === "undefined") {
    JobPosting.Form = {};
}

if (typeof (JobPosting.Ribbon) === "undefined") {
    JobPosting.Ribbon = {};
}

JobPosting.Form = {
    config: {
        statusField: "statuscode",
        publishedStatusValue: 755700001,
        manpowerRequestField: "bcc_manpowerrequestid",
        positionField: "bcc_position",
        workforcePlanningField: "bcc_workforceplanningid"
    },

    isPublished: function (formContext) {
        try {
            var statusAttr = formContext.getAttribute(JobPosting.Form.config.statusField);
            return !!statusAttr && statusAttr.getValue() === JobPosting.Form.config.publishedStatusValue;
        } catch (error) {
            return false;
        }
    },

    lockAllFields: function (formContext) {
        try {
            if (!formContext || !formContext.ui || !formContext.ui.controls) {
                return;
            }

            formContext.ui.controls.forEach(function (control) {
                if (control && control.setDisabled) {
                    control.setDisabled(true);
                }
            });
        } catch (error) {
            console.error("JobPosting.Form.lockAllFields - unexpected error: " + error.message);
        }
    },

    onFormLoad: function (executionContext) {
        try {
            var formContext = executionContext.getFormContext
                ? executionContext.getFormContext()
                : executionContext;

            var manpowerRequestAttr = formContext.getAttribute(JobPosting.Form.config.manpowerRequestField);
            if (manpowerRequestAttr && !manpowerRequestAttr.__bccgulfManpowerRequestHandlerAdded) {
                manpowerRequestAttr.addOnChange(JobPosting.Form.onChangeManPowerRequest);
                manpowerRequestAttr.__bccgulfManpowerRequestHandlerAdded = true;
            }
            debugger;
            JobPosting.Form.onChangeManPowerRequest(executionContext);

            if (JobPosting.Form.isPublished(formContext)) {
                JobPosting.Form.lockAllFields(formContext);
            }
        } catch (error) {
            console.error("JobPosting.Form.onFormLoad - unexpected error: " + error.message);
        }
    },

    onChangeManPowerRequest: async function (executionContext) {
        try {
            debugger;
            var formContext = executionContext.getFormContext
                ? executionContext.getFormContext()
                : executionContext;

            var manpowerRequestAttr = formContext.getAttribute(JobPosting.Form.config.manpowerRequestField);
            var positionAttr = formContext.getAttribute(JobPosting.Form.config.positionField);
            var workforcePlanningAttr = formContext.getAttribute(JobPosting.Form.config.workforcePlanningField);

            if (!manpowerRequestAttr) {
                return;
            }

            var manpowerRequestValue = manpowerRequestAttr.getValue();
            if (!manpowerRequestValue || manpowerRequestValue.length === 0) {
                positionAttr.setValue(null);
                workforcePlanningAttr.setValue(null);
                return;
            }

            var manpowerRequestId = manpowerRequestValue[0].id.replace("{", "").replace("}", "");
            var record = await Xrm.WebApi.retrieveRecord(
                "bcc_manpowerrequest",
                manpowerRequestId,
                "?$select=_bcc_designation_value,_bcc_workforceplanningid_value"
            );

            var positionValue = null;
            var workforcePlanningValue = null;

            if (record && record._bcc_designation_value) {
                positionValue = [{
                    id: record._bcc_designation_value,
                    name: record["_bcc_designation_value@OData.Community.Display.V1.FormattedValue"] || "",
                    entityType: "bcc_designation"
                }];
            }

            if (record && record._bcc_workforceplanningid_value) {
                workforcePlanningValue = [{
                    id: record._bcc_workforceplanningid_value,
                    name: record["_bcc_workforceplanningid_value@OData.Community.Display.V1.FormattedValue"] || "",
                    entityType: "bcc_workforceplanning"
                }];
            }

            positionAttr.setValue(positionValue);
            workforcePlanningAttr.setValue(workforcePlanningValue);
        } catch (error) {
            console.error("JobPosting.Form.onChangeManPowerRequest - unexpected error: " + error.message);
        }
    }
};

JobPosting.Ribbon = {
    onClickPublish: function (FormContext) {
        try {
            FormContext.getAttribute("statuscode").setValue(755700001); // Set status to Published
            FormContext.data.entity.save().then(
                function () {
                    try {
                        JobPosting.Form.lockAllFields(FormContext);
                    } catch (error) {
                        console.error("JobPosting.Ribbon.onClickPublish - lock error: " + error.message);
                    }
                },
                function (error) {
                    console.error("Error saving Job Posting: " + error.message);
                }
            );
        }
        catch (error) {
            console.error("Error in onClickPublish: " + error.message);
        }
    }
};