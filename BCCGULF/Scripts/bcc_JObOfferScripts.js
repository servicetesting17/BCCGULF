var BCCJobOffer = BCCJobOffer || {};

BCCJobOffer.Form = {
    config: {
        airTicketField: "bcc_airticket",
        airTicketAttachmentField: "bcc_attatchairticket",
        subStageStatusField: "bcc_substagestatus",
        biometricStages: ["Biometric Appointment", "Biometric Done"],
        biometricSubStageStatusValue: 755700000,
        biometricStatusValue: 755700019,
        designationField: "bcc_designation",
        noAccessNotificationId: "bccJobOfferNoAccess",
        noAccessMessage: "You do not have access to this record."
    },

    onLoad: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var config = BCCJobOffer.Form.config;

        // Only members of a team whose configuration covers the designation (or an admin configuration)
        // may see the record. Applies on Create as well as on existing records.
        BCCJobOffer.Form.getTeamAccess().then(function (access) {
            var designationId = BCCJobOffer.Form.getDesignationId(formContext);
            var designationAllowed = designationId !== null && access.designationIds.indexOf(designationId) !== -1;
            var hasAccess;

            if (formContext.ui.getFormType() === 1) {
                // Create: Designation is usually still empty, so allow anyone with at least one team
                // designation and restrict the lookup to those designations
                hasAccess = access.isAdmin ||
                    (access.designationIds.length > 0 && (designationId === null || designationAllowed));
                if (hasAccess && !access.isAdmin) {
                    BCCJobOffer.Form.filterDesignationLookup(formContext, access.designationIds);
                }
            } else {
                hasAccess = access.isAdmin || designationAllowed;
            }

            console.log("BCCJobOffer.Form.onLoad - admin: " + access.isAdmin +
                ", team designations: " + access.designationIds.length + ", access: " + hasAccess);

            if (hasAccess) {
                BCCJobOffer.Form.initForm(executionContext);
            } else {
                BCCJobOffer.Form.hideFormForNoAccess(formContext);
            }
        });
    },

    getDesignationId: function (formContext) {
        var designationAttr = formContext.getAttribute(BCCJobOffer.Form.config.designationField);
        var designationValue = designationAttr ? designationAttr.getValue() : null;
        return designationValue && designationValue.length > 0
            ? designationValue[0].id.replace(/[{}]/g, "").toLowerCase()
            : null;
    },

    /**
     * Looks at the active BCC Configuration records owned by the current user's teams
     * (default business unit teams excluded) and resolves
     * { isAdmin: any of them has the Admin flag, designationIds: designations linked to them }.
     * Fails closed (no admin, no designations) if the queries fail.
     */
    getTeamAccess: function () {
        var userId = Xrm.Utility.getGlobalContext().userSettings.userId.replace(/[{}]/g, "");

        // Configuration must be active and owned by a non-default team the user is a member of
        var userTeamConfigFilter = [
            "<filter type='and'>",
            "  <condition attribute='statecode' operator='eq' value='0' />",
            "</filter>",
            "<link-entity name='team' from='teamid' to='owningteam'>",
            "  <filter type='and'>",
            "    <condition attribute='isdefault' operator='eq' value='0' />",
            "  </filter>",
            "  <link-entity name='teammembership' from='teamid' to='teamid' intersect='true'>",
            "    <filter type='and'>",
            "      <condition attribute='systemuserid' operator='eq' value='" + userId + "' />",
            "    </filter>",
            "  </link-entity>",
            "</link-entity>"
        ].join("");

        // Team configuration records with the Admin flag (admin configurations have no designations)
        var adminCheck = Xrm.WebApi.retrieveMultipleRecords("bcc_bccconfiguration", "?fetchXml=" + encodeURIComponent([
            "<fetch version='1.0' distinct='true'>",
            "  <entity name='bcc_bccconfiguration'>",
            "    <attribute name='bcc_bccconfigurationid' />",
            "    <attribute name='bcc_admin' />",
            userTeamConfigFilter,
            "  </entity>",
            "</fetch>"
        ].join(""))).then(function (result) {
            return result.entities.some(function (record) {
                return BCCJobOffer.Form.isYes(record, "bcc_admin");
            });
        });

        // Designations linked to the team configuration records
        var designationQuery = Xrm.WebApi.retrieveMultipleRecords("bcc_designation", "?fetchXml=" + encodeURIComponent([
            "<fetch version='1.0' distinct='true'>",
            "  <entity name='bcc_designation'>",
            "    <attribute name='bcc_designationid' />",
            "    <link-entity name='bcc_bccconfiguration_bcc_designation' intersect='true' visible='false'",
            "        from='bcc_designationid' to='bcc_designationid'>",
            "      <link-entity name='bcc_bccconfiguration' from='bcc_bccconfigurationid' to='bcc_bccconfigurationid'>",
            userTeamConfigFilter,
            "      </link-entity>",
            "    </link-entity>",
            "  </entity>",
            "</fetch>"
        ].join(""))).then(function (result) {
            return result.entities.map(function (record) {
                return record.bcc_designationid.replace(/[{}]/g, "").toLowerCase();
            });
        });

        return Promise.all([adminCheck, designationQuery]).then(function (results) {
            return { isAdmin: results[0], designationIds: results[1] };
        }).catch(function (error) {
            // Fail closed: if access can't be verified, don't show the record
            console.error("BCCJobOffer.Form.getTeamAccess - " + error.message);
            return { isAdmin: false, designationIds: [] };
        });
    },

    // Limits the Designation lookup to the designations of the user's team configurations
    filterDesignationLookup: function (formContext, designationIds) {
        var designationAttr = formContext.getAttribute(BCCJobOffer.Form.config.designationField);
        if (!designationAttr) {
            console.error("BCCJobOffer.Form.filterDesignationLookup - field not found: " +
                BCCJobOffer.Form.config.designationField);
            return;
        }

        var filterXml = "<filter type='and'><condition attribute='bcc_designationid' operator='in'>" +
            designationIds.map(function (id) { return "<value>" + id + "</value>"; }).join("") +
            "</condition></filter>";

        designationAttr.controls.forEach(function (control) {
            if (control.__bccDesignationFilterAdded) {
                return;
            }
            control.addPreSearch(function () {
                control.addCustomFilter(filterXml, "bcc_designation");
            });
            control.__bccDesignationFilterAdded = true;
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
