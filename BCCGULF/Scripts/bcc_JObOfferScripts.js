var BCCJobOffer = BCCJobOffer || {};

BCCJobOffer.Form = {
    config: {
        airTicketField: "bcc_airticket",
        airTicketAttachmentField: "bcc_attatchairticket",
        subStageStatusField: "bcc_substagestatus",
        biometricStages: ["Biometric Appointment", "Biometric Done"],
        biometricSubStageStatusValue: 755700000,
        biometricStatusValue: 755700019,
        renewalStatusField: "bcc_renewalstatus",
        designationField: "bcc_designation",
        noAccessNotificationId: "bccJobOfferNoAccess",
        noAccessMessage: "You do not have access to this record."
    },

    onLoad: function (executionContext) {
        var formContext = executionContext.getFormContext();

        // Renewal Job Offers: only users configured for the designation (or admins) may see the record.
        // Skipped on Create (form type 1) — only existing records are checked.
        var isCreateForm = formContext.ui.getFormType() === 1;
        if (!isCreateForm && BCCJobOffer.Form.hasRenewalStatus(formContext)) {
            BCCJobOffer.Form.checkRenewalAccess(formContext).then(function (hasAccess) {
                if (hasAccess) {
                    BCCJobOffer.Form.initForm(executionContext);
                } else {
                    BCCJobOffer.Form.hideFormForNoAccess(formContext);
                }
            });
            return;
        }

        BCCJobOffer.Form.initForm(executionContext);
    },

    hasRenewalStatus: function (formContext) {
        var renewalStatusAttr = formContext.getAttribute(BCCJobOffer.Form.config.renewalStatusField);
        if (!renewalStatusAttr) {
            return false;
        }
        var value = renewalStatusAttr.getValue();
        return value !== null && value !== undefined && value !== "";
    },

    /**
     * Resolves true when the current user has an active BCC Configuration record that is either
     * linked to the Job Offer's designation, or has the Admin flag set.
     */
    checkRenewalAccess: function (formContext) {
        var config = BCCJobOffer.Form.config;
        var userId = Xrm.Utility.getGlobalContext().userSettings.userId.replace(/[{}]/g, "");

        var designationAttr = formContext.getAttribute(config.designationField);
        var designationValue = designationAttr ? designationAttr.getValue() : null;
        var designationId = designationValue && designationValue.length > 0
            ? designationValue[0].id.replace(/[{}]/g, "")
            : null;

        // User's configuration records linked to the Job Offer designation
        var designationCheck = Promise.resolve(0);
        if (designationId) {
            var fetchXml = [
                "<fetch version='1.0' distinct='true'>",
                "  <entity name='bcc_bccconfiguration'>",
                "    <attribute name='bcc_bccconfigurationid' />",
                "    <attribute name='bcc_admin' />",
                "    <order attribute='bcc_name' descending='false' />",
                "    <filter type='and'>",
                "      <condition attribute='statecode' operator='eq' value='0' />",
                "      <condition attribute='bcc_userid' operator='eq' value='" + userId + "' />",
                "    </filter>",
                "    <link-entity name='bcc_bccconfiguration_bcc_designation' intersect='true' visible='false'",
                "        from='bcc_bccconfigurationid' to='bcc_bccconfigurationid'>",
                "      <link-entity name='bcc_designation' alias='aa' from='bcc_designationid' to='bcc_designationid'>",
                "        <filter type='and'>",
                "          <condition attribute='bcc_designationid' operator='eq' value='" + designationId + "' />",
                "        </filter>",
                "      </link-entity>",
                "    </link-entity>",
                "  </entity>",
                "</fetch>"
            ].join("");

            designationCheck = Xrm.WebApi.retrieveMultipleRecords("bcc_bccconfiguration", "?fetchXml=" + encodeURIComponent(fetchXml))
                .then(function (result) { return result.entities.length; });
        }

        // User's configuration records with the Admin flag (any designation)
        var adminCheck = Xrm.WebApi.retrieveMultipleRecords("bcc_bccconfiguration",
            "?$select=bcc_admin&$filter=statecode eq 0 and _bcc_userid_value eq " + userId)
            .then(function (result) {
                return result.entities.some(function (record) {
                    return BCCJobOffer.Form.isYes(record, "bcc_admin");
                });
            });

        return Promise.all([designationCheck, adminCheck]).then(function (results) {
            var designationCount = results[0];
            var isAdmin = results[1];
            console.log("BCCJobOffer.Form.checkRenewalAccess - designation matches: " + designationCount + ", admin: " + isAdmin);
            return designationCount > 0 || isAdmin;
        }).catch(function (error) {
            // Fail closed: if access can't be verified, don't show the record
            console.error("BCCJobOffer.Form.checkRenewalAccess - " + error.message);
            return false;
        });
    },

    // Admin flag may be a Yes/No column (true) or a choice whose label is "Yes"
    isYes: function (record, fieldName) {
        var value = record[fieldName];
        var label = record[fieldName + "@OData.Community.Display.V1.FormattedValue"];
        return value === true || value === 1 || (typeof label === "string" && label.toLowerCase() === "yes");
    },

    hideFormForNoAccess: function (formContext) {
        var ui = formContext.ui;

        ui.tabs.forEach(function (tab) {
            tab.setVisible(false);
        });

        if (ui.process && typeof ui.process.setVisible === "function") {
            ui.process.setVisible(false);
        }

        if (ui.headerSection) {
            if (typeof ui.headerSection.setBodyVisible === "function") ui.headerSection.setBodyVisible(false);
            if (typeof ui.headerSection.setTabNavigatorVisible === "function") ui.headerSection.setTabNavigatorVisible(false);
            if (typeof ui.headerSection.setCommandBarVisible === "function") ui.headerSection.setCommandBarVisible(false);
        }

        ui.setFormNotification(BCCJobOffer.Form.config.noAccessMessage, "ERROR", BCCJobOffer.Form.config.noAccessNotificationId);
    },

    // Existing form load logic — runs only when the user may see the record
    initForm: function (executionContext) {
        var formContext = executionContext.getFormContext();
        BCCJobOffer.Form.setBpfForStatus(formContext);

        if (formContext.data && formContext.data.process && !BCCJobOffer.Form.__stageChangeHandlerAdded) {
            formContext.data.process.addOnStageChange(BCCJobOffer.Form.onStageChange);
            BCCJobOffer.Form.__stageChangeHandlerAdded = true;
        }
       
        BCCJobOffer.Form.onStageChange(executionContext);
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
     * BPF stage change: on Biometric Appointment / Biometric Done set the sub stage status
     * and the status reason, then save.
     */
    onStageChange: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var config = BCCJobOffer.Form.config;
        // Stage change passes the new stage in event args; on form load fall back to the active stage
        var eventArgs = executionContext.getEventArgs ? executionContext.getEventArgs() : null;
        var stage = eventArgs && typeof eventArgs.getStage === "function" ? eventArgs.getStage() : null;
        if (!stage && formContext.data && formContext.data.process) {
            stage = formContext.data.process.getActiveStage();
        }
        var stageName = stage ? stage.getName() : null;

        if (config.biometricStages.indexOf(stageName) === -1) {
            return;
        }

        var subStageStatusAttr = formContext.getAttribute(config.subStageStatusField);
        if (!subStageStatusAttr) {
            console.error("BCCJobOffer.Form.onStageChange - field not found: " + config.subStageStatusField);
            return;
        }
        var changed = false;

        if (subStageStatusAttr.getValue() !== config.biometricSubStageStatusValue) {
            subStageStatusAttr.setValue(config.biometricSubStageStatusValue);
            changed = true;
        }

        var statusAttr = formContext.getAttribute("statuscode");
        if (!statusAttr) {
            console.error("BCCJobOffer.Form.onStageChange - field not found: statuscode");
            return;
        }
        if (statusAttr.getValue() !== config.biometricStatusValue) {
            statusAttr.setValue(config.biometricStatusValue);
            changed = true;
        }

        // Only save when a value actually changed, so form load doesn't save every time
        if (!changed) {
            return;
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
