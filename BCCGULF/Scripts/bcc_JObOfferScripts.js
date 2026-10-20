var BCCJobOffer = BCCJobOffer || {};

BCCJobOffer.Form = {
    config: {
        airTicketField: "bcc_airticket",
        airTicketAttachmentField: "bcc_attatchairticket",
        subStageStatusField: "bcc_substagestatus",
        biometricStages: ["Biometric Appointment", "Biometric Done"],
        biometricDoneStage: "Biometric Done",
        biometricSubStageStatusValue: 755700000,
        biometricDoneStatusValue: 755700019
    },

    onLoad: function (executionContext) {
        var formContext = executionContext.getFormContext();
        BCCJobOffer.Form.setBpfForStatus(formContext);

        if (formContext.data && formContext.data.process && !BCCJobOffer.Form.__stageChangeHandlerAdded) {
            formContext.data.process.addOnStageChange(BCCJobOffer.Form.onStageChange);
            BCCJobOffer.Form.__stageChangeHandlerAdded = true;
        }

        var airTicketAttr = formContext.getAttribute(BCCJobOffer.Form.config.airTicketField);

        if (!airTicketAttr) {
            console.error("BCCJobOffer.Form.onLoad - field not found: " +
                BCCJobOffer.Form.config.airTicketField);
            return;
        }

        if (!airTicketAttr.__bccJobOfferAirTicketHandlerAdded) {
            airTicketAttr.addOnChange(BCCJobOffer.Form.onAirTicketChange);
            airTicketAttr.__bccJobOfferAirTicketHandlerAdded = true;
        }

        BCCJobOffer.Form.onAirTicketChange(executionContext);
    },

    setBpfForStatus: function (formContext) {
        var statusAttr = formContext.getAttribute("statuscode");
        if (!statusAttr) {
            console.error("BCCJobOffer.Form.setBpfForStatus - field not found: statuscode");
            return;
        }
        if (!formContext.data || !formContext.data.process ||
            typeof formContext.data.process.setActiveProcess !== "function") {
            console.error("BCCJobOffer.Form.setBpfForStatus - BPF process control is unavailable.");
            return;
        }

        var statusText = statusAttr.getText();
        var processUniqueName = statusText === "Cancellation Initiated" || statusText === "Cancelled"
            ? "bcc_cancellationbpf"
            : "bcc_approval";
        var query = "?$select=workflowid&$filter=uniquename eq '" +
            processUniqueName + "' and category eq 4 and statecode eq 1";

        Xrm.WebApi.retrieveMultipleRecords("workflow", query).then(function (result) {
            if (result.entities.length !== 1) {
                throw new Error(
                    "Expected one active BPF (" + processUniqueName +
                    "), but found " + result.entities.length + "."
                );
            }

            formContext.data.process.setActiveProcess(
                result.entities[0].workflowid.toUpperCase(),
                function (status) {
                    if (status !== "success") {
                        console.error(
                            "BCCJobOffer.Form.setBpfForStatus - could not activate " +
                            processUniqueName + " (" + status + ")."
                        );
                    }
                }
            );
        }).catch(function (error) {
            console.error("BCCJobOffer.Form.setBpfForStatus - " + error.message);
        });
    },

    /**
     * BPF stage change: on Biometric Appointment / Biometric Done set the sub stage status,
     * and on Biometric Done also set the status reason, then save.
     */
    onStageChange: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var config = BCCJobOffer.Form.config;
        var stage = executionContext.getEventArgs().getStage() || formContext.data.process.getActiveStage();
        var stageName = stage ? stage.getName() : null;

        if (config.biometricStages.indexOf(stageName) === -1) {
            return;
        }

        var subStageStatusAttr = formContext.getAttribute(config.subStageStatusField);
        if (!subStageStatusAttr) {
            console.error("BCCJobOffer.Form.onStageChange - field not found: " + config.subStageStatusField);
            return;
        }
        subStageStatusAttr.setValue(config.biometricSubStageStatusValue);

        if (stageName === config.biometricDoneStage) {
            var statusAttr = formContext.getAttribute("statuscode");
            if (!statusAttr) {
                console.error("BCCJobOffer.Form.onStageChange - field not found: statuscode");
                return;
            }
            statusAttr.setValue(config.biometricDoneStatusValue);
        }

        formContext.data.save().then(null, function (error) {
            console.error("BCCJobOffer.Form.onStageChange - save failed: " + error.message);
        });
    },

    onAirTicketChange: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var airTicketAttr = formContext.getAttribute(BCCJobOffer.Form.config.airTicketField);
        var attachmentAttr = formContext.getAttribute(BCCJobOffer.Form.config.airTicketAttachmentField);

        if (!airTicketAttr || !attachmentAttr) {
            console.error("BCCJobOffer.Form.onAirTicketChange - required field not found.");
            return;
        }

        var airTicketRequired = airTicketAttr.getValue() === true;
        attachmentAttr.setRequiredLevel(airTicketRequired ? "required" : "none");

        attachmentAttr.controls.forEach(function (control) {
            control.setVisible(airTicketRequired);
        });
    }
};
