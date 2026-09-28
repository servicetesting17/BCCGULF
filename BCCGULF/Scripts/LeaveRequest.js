if (typeof (LeaveRequest) === "undefined") {
    var LeaveRequest = {};
}

if (typeof (LeaveRequest.Form) === "undefined") {
    LeaveRequest.Form = {};
}

if (typeof (LeaveRequest.Ribbon) === "undefined") {
    LeaveRequest.Ribbon = {};
}

// ✅ Leave Request Form Operations
LeaveRequest.Form = {
    config: {
        resourceLogicalName: "bookableresource",
        leaveTypeLogicalName: "bcc_leavetype",
        leaveBalanceLogicalName: "bcc_leavebalance",
        holidayLogicalName: "bcc_holidays",
        resourceField: "bcc_bookableresourceid",
        leaveTypeField: "bcc_leavetypeid",
        leaveBalanceField: "bcc_leavebalance",
        startTimeField: "bcc_starttime",
        endTimeField: "bcc_endtime",
        numberOfDaysField: "bcc_leavedays",
        leaveBalanceValueField: "bcc_leavebalanceid",
        leaveBalanceNameField: "bcc_name",
        leaveRequestReplaceMentField: "bcc_replacementstaffid",
        holidayStartDateField: "bcc_holidaydate",
        holidayEndDateField: "bcc_holidayenddate",
        attachmentRequiredField: "bcc_attachmentrequired",
        attachmentField: "bcc_attachment",
        staffResourceTypeValue: 100000001  // option-set value of "Staff" in the new_type field — update if different
    },

    /**
     * Form Load Handler
     */
    onFormLoad: function (executionContext) {
        try {
            debugger;
            var formContext = executionContext.getFormContext ? executionContext.getFormContext() : executionContext;

            // Add change event handler to Bookable Resource field
            var resourceField = formContext.getAttribute(LeaveRequest.Form.config.resourceField);
            if (resourceField) {
                resourceField.addOnChange(LeaveRequest.Form.onResourceFieldChange);

                // Check if resource is already selected on form load
                var resourceLookup = resourceField.getValue();
                if (resourceLookup && resourceLookup.length > 0) {
                    var resourceId = resourceLookup[0].id;
                    LeaveRequest.Form.fetchResourceById(resourceId, formContext);
                }
            }

            // Add change event handlers to Start Time and End Time fields
            var startTimeField = formContext.getAttribute(LeaveRequest.Form.config.startTimeField);
            if (startTimeField) {
                startTimeField.addOnChange(LeaveRequest.Form.onDateFieldChange);
            }

            var endTimeField = formContext.getAttribute(LeaveRequest.Form.config.endTimeField);
            if (endTimeField) {
                endTimeField.addOnChange(LeaveRequest.Form.onDateFieldChange);
            }

            var leaveTypeField = formContext.getAttribute(LeaveRequest.Form.config.leaveTypeField);
            if (leaveTypeField) {
                leaveTypeField.addOnChange(LeaveRequest.Form.onLeaveTypeFieldChange);
            }

            LeaveRequest.Form.loadLeaveBalance(formContext);

            // Check if leave type has attachment required on form load
            var leaveTypeAttr = formContext.getAttribute(LeaveRequest.Form.config.leaveTypeField);
            var numberOfDaysAttr = formContext.getAttribute(LeaveRequest.Form.config.numberOfDaysField).getValue();

            if (leaveTypeAttr) {
                var leaveTypeLookup = leaveTypeAttr.getValue();
                if (leaveTypeLookup && leaveTypeLookup.length > 0) {
                    var leaveTypeId = LeaveRequest.Form.normalizeGuid(leaveTypeLookup[0].id);
                    LeaveRequest.Form.checkLeaveTypeAttachmentRequired(leaveTypeId, formContext);
                } else {
                    LeaveRequest.Form.setAttachmentFieldVisibility(formContext, false);
                }
            }

            // If Create form, clear the replacement staff field
            if (formContext.ui.getFormType() === 1) {
                var replacementAttr = formContext.getAttribute(LeaveRequest.Form.config.leaveRequestReplaceMentField);
                if (replacementAttr) {
                    replacementAttr.setValue(null);
                }
            }

            // Check form type and disable fields if not in Create mode
            LeaveRequest.Form.checkFormTypeAndDisableFields(formContext);

            console.log("LeaveRequest Form loaded successfully");
        } catch (error) {
            console.error("Error on Form Load: " + error.message);
        }
    },

    /**
     * Check Form Type and Disable Fields if Not in Create Mode
     * Form Type: 1 = Create, 2 = Update, 3 = Read-only, 4 = Disabled, 6 = Bulk Edit
     */
    checkFormTypeAndDisableFields: function (formContext) {
        try {
        var statusCode = formContext.getAttribute("statuscode").getValue();

            if (statusCode !== 755700002) {
                console.log("Form is not in Create mode. Disabling all fields.");

                // Iterate through all controls and disable them
                formContext.ui.controls.forEach(function (control) {
                    try {
                        if (control.setDisabled) {
                            control.setDisabled(true);
                        }
                    } catch (error) {
                        console.warn("Could not disable control: " + control.getName());
                    }
                });

                console.log("All fields have been disabled.");
            } else {
                console.log("Form is in Create mode. Fields remain editable.");
            }
        } catch (error) {
            console.error("Error in checkFormTypeAndDisableFields: " + error.message);
        }
    },

    /**
     * Start/End Date Field Change Event Handler
     */
    onDateFieldChange: function (executionContext) {
        try {
            var formContext = executionContext.getFormContext();
            LeaveRequest.Form.calculateNumberOfDays(formContext);
        } catch (error) {
            console.error("Error on Date Field Change: " + error.message);
        }
    },

    /**
     * Calculate Number of Days between Start Time and End Time.
     * A day ending at or before 1 PM (13:00) counts as a half day (0.5),
     * otherwise it counts as a full day (1.0).
     * The start time uses the same rule for the first day.
     */
    calculateNumberOfDays: function (formContext) {
        try {
            var startAttr = formContext.getAttribute(LeaveRequest.Form.config.startTimeField);
            var endAttr = formContext.getAttribute(LeaveRequest.Form.config.endTimeField);
            var daysAttr = formContext.getAttribute(LeaveRequest.Form.config.numberOfDaysField);

            if (!startAttr || !endAttr || !daysAttr) return;

            var startValue = startAttr.getValue();
            var endValue = endAttr.getValue();

            if (!startValue || !endValue) {
                daysAttr.setValue(null);
                return;
            }

            var startDate = new Date(startValue);
            var endDate = new Date(endValue);

            if (endDate < startDate) {
                daysAttr.setValue(null);
                console.warn("End Time is before Start Time");
                return;
            }

            var leaveTypeAttr = formContext.getAttribute(LeaveRequest.Form.config.leaveTypeField);
            var leaveTypeValue = leaveTypeAttr ? leaveTypeAttr.getValue() : null;
            var leaveTypeName = leaveTypeValue && leaveTypeValue.length > 0
                ? (leaveTypeValue[0].name || "")
                : "";
            var isEmergencyLeave = leaveTypeName.toLowerCase().indexOf("emergency") !== -1;
            var isStaff = (LeaveRequest.Form.currentResourceTypeValue === LeaveRequest.Form.config.staffResourceTypeValue);

            if (isEmergencyLeave) {
                var emergencyTotalDays = LeaveRequest.Form.calculateLeaveDaysExcludingNonWorkingDays(
                    startDate,
                    endDate,
                    {},
                    false
                );
                daysAttr.setValue(emergencyTotalDays);
                console.log("Emergency leave days calculated without holidays or weekends: " + emergencyTotalDays);
                return;
            }

            LeaveRequest.Form.fetchHolidayDates(startDate, endDate)
                .then(function (holidayDates) {
                    var totalDays = LeaveRequest.Form.calculateLeaveDaysExcludingNonWorkingDays(startDate, endDate, holidayDates, isStaff);
                    daysAttr.setValue(totalDays);
                    console.log("Number of days calculated: " + totalDays + " (excludeWeekends=" + isStaff + ")");
                })
                .catch(function (error) {
                    console.error("Error fetching holidays: " + error.message);
                    daysAttr.setValue(null);
                });
        } catch (error) {
            console.error("Error in calculateNumberOfDays: " + error.message);
        }
    },

    /**
     * Calculate leave days excluding holidays, and weekends only for Staff resource type.
     * @param {boolean} excludeWeekends - true when the bookable resource type is Staff
     */
    calculateLeaveDaysExcludingNonWorkingDays: function (startDate, endDate, holidayDates, excludeWeekends) {
        var HALF_DAY_HOUR = 13; // 1 PM
        var totalDays = 0;
        var current = new Date(startDate);
        current.setHours(0, 0, 0, 0);

        var endDay = new Date(endDate);
        endDay.setHours(0, 0, 0, 0);

        while (current <= endDay) {
            var currentDateKey = LeaveRequest.Form.getDateKey(current);
            var dayOfWeek = current.getDay();
            var isWeekend = (dayOfWeek === 0 || dayOfWeek === 6) && excludeWeekends;
            var isHoliday = holidayDates[currentDateKey] === true && excludeWeekends;

            if (isWeekend || isHoliday) {
                current.setDate(current.getDate() + 1);
                continue;
            }

            var isFirstDay = (current.toDateString() === startDate.toDateString());
            var isLastDay = (current.toDateString() === endDate.toDateString());

            if (isFirstDay && isLastDay) {
                totalDays += (endDate.getHours() < HALF_DAY_HOUR ||
                    (endDate.getHours() === HALF_DAY_HOUR && endDate.getMinutes() === 0))
                    ? 0.5 : 1;
            } else if (isFirstDay) {
                totalDays += (startDate.getHours() >= HALF_DAY_HOUR) ? 0.5 : 1;
            } else if (isLastDay) {
                totalDays += (endDate.getHours() < HALF_DAY_HOUR ||
                    (endDate.getHours() === HALF_DAY_HOUR && endDate.getMinutes() === 0))
                    ? 0.5 : 1;
            } else {
                totalDays += 1;
            }

            current.setDate(current.getDate() + 1);
        }

        return totalDays;
    },

    /**
     * Fetch holiday dates overlapping the selected leave range
     */
    fetchHolidayDates: function (startDate, endDate) {
        try {
            var config = LeaveRequest.Form.config;
            var holidayDates = {};
            var startDateValue = LeaveRequest.Form.toFetchXmlDate(startDate);
            var endDateValue = LeaveRequest.Form.toFetchXmlDate(endDate);
            var fetchXml = [
                "<fetch>",
                "  <entity name='" + config.holidayLogicalName + "'>",
                "    <attribute name='" + config.holidayStartDateField + "' />",
                "    <attribute name='" + config.holidayEndDateField + "' />",
                "    <filter type='and'>",
                "      <condition attribute='" + config.holidayStartDateField + "' operator='on-or-before' value='" + endDateValue + "' />",
                "      <condition attribute='" + config.holidayEndDateField + "' operator='on-or-after' value='" + startDateValue + "' />",
                "    </filter>",
                "  </entity>",
                "</fetch>"
            ].join("");

            return Xrm.WebApi.retrieveMultipleRecords(config.holidayLogicalName, "?fetchXml=" + encodeURIComponent(fetchXml))
                .then(function (result) {
                    result.entities.forEach(function (holiday) {
                        var holidayStart = holiday[config.holidayStartDateField];
                        var holidayEnd = holiday[config.holidayEndDateField] || holidayStart;

                        if (!holidayStart) {
                            return;
                        }

                        var currentHolidayDate = new Date(holidayStart);
                        currentHolidayDate.setHours(0, 0, 0, 0);

                        var holidayEndDate = new Date(holidayEnd);
                        holidayEndDate.setHours(0, 0, 0, 0);

                        while (currentHolidayDate <= holidayEndDate) {
                            holidayDates[LeaveRequest.Form.getDateKey(currentHolidayDate)] = true;
                            currentHolidayDate.setDate(currentHolidayDate.getDate() + 1);
                        }
                    });

                    return holidayDates;
                });
        } catch (error) {
            return Promise.reject(error);
        }
    },

    /**
     * Get normalized date key for comparison
     */
    getDateKey: function (value) {
        var normalizedDate = new Date(value);
        normalizedDate.setHours(0, 0, 0, 0);
        return normalizedDate.getFullYear() + "-" +
            String(normalizedDate.getMonth() + 1).padStart(2, "0") + "-" +
            String(normalizedDate.getDate()).padStart(2, "0");
    },

    /**
     * Convert a date value to FetchXML date format
     */
    toFetchXmlDate: function (value) {
        var normalizedDate = new Date(value);
        normalizedDate.setHours(0, 0, 0, 0);
        return normalizedDate.toISOString();
    },

    /**
     * Bookable Resource Field Change Event Handler
     */
    onResourceFieldChange: function (executionContext) {
        try {
            debugger;
            var formContext = executionContext.getFormContext();
            var resourceLookup = formContext.getAttribute(LeaveRequest.Form.config.resourceField).getValue();

            if (!resourceLookup || resourceLookup.length === 0) {
                LeaveRequest.Form.clearLeaveTypeFilter(formContext);
                LeaveRequest.Form.clearLeaveBalance(formContext);
                return;
            }

            var resourceId = resourceLookup[0].id;

            // Clear existing leave type value only when user changes resource
            formContext.getAttribute(LeaveRequest.Form.config.leaveTypeField).setValue(null);
            LeaveRequest.Form.clearLeaveBalance(formContext);

            LeaveRequest.Form.fetchResourceById(resourceId, formContext);
        } catch (error) {
            console.error("Error on Resource Field Change: " + error.message);
        }
    },

    /**
     * Leave Type Field Change Event Handler
     */
    onLeaveTypeFieldChange: function (executionContext) {
        try {
            var formContext = executionContext.getFormContext();
            LeaveRequest.Form.loadLeaveBalance(formContext);

            // Check if leave type has attachment required
            var leaveTypeAttr = formContext.getAttribute(LeaveRequest.Form.config.leaveTypeField);
            var numberOfDaysAttr = formContext.getAttribute(LeaveRequest.Form.config.numberOfDaysField);

            if (leaveTypeAttr) {
                var leaveTypeLookup = leaveTypeAttr.getValue();
                if (leaveTypeLookup && leaveTypeLookup.length > 0) {
                    var leaveTypeId = LeaveRequest.Form.normalizeGuid(leaveTypeLookup[0].id);
                    LeaveRequest.Form.checkLeaveTypeAttachmentRequired(leaveTypeId, formContext);
                } else {
                    LeaveRequest.Form.setAttachmentFieldVisibility(formContext, false);
                }
            }
        } catch (error) {
            console.error("Error on Leave Type Field Change: " + error.message);
        }
    },

    /**
     * Load Leave Balance based on Resource and Leave Type
     */
    loadLeaveBalance: function (formContext) {
        try {
            var resourceAttr = formContext.getAttribute(LeaveRequest.Form.config.resourceField);
            var leaveTypeAttr = formContext.getAttribute(LeaveRequest.Form.config.leaveTypeField);

            if (!resourceAttr || !leaveTypeAttr) {
                return;
            }

            var resourceLookup = resourceAttr.getValue();
            var leaveTypeLookup = leaveTypeAttr.getValue();

            if (!resourceLookup || resourceLookup.length === 0 || !leaveTypeLookup || leaveTypeLookup.length === 0) {
                LeaveRequest.Form.clearLeaveBalance(formContext);
                return;
            }

            var resourceId = LeaveRequest.Form.normalizeGuid(resourceLookup[0].id);
            var leaveTypeId = LeaveRequest.Form.normalizeGuid(leaveTypeLookup[0].id);

            LeaveRequest.Form.fetchLeaveBalance(resourceId, leaveTypeId, formContext);
        } catch (error) {
            console.error("Error in loadLeaveBalance: " + error.message);
        }
    },

    /**
     * Fetch Leave Balance record by Resource and Leave Type
     */
    fetchLeaveBalance: function (resourceId, leaveTypeId, formContext) {
        try {
            var config = LeaveRequest.Form.config;
            var fetchXml = [
                "<fetch top='1'>",
                "  <entity name='" + config.leaveBalanceLogicalName + "'>",
                "    <attribute name='" + config.leaveBalanceValueField + "' />",
                "    <attribute name='" + config.leaveBalanceNameField + "' />",
                "    <filter type='and'>",
                "      <condition attribute='" + config.resourceField + "' operator='eq' value='" + resourceId + "' />",
                "      <condition attribute='" + config.leaveTypeField + "' operator='eq' value='" + leaveTypeId + "' />",
                "    </filter>",
                "  </entity>",
                "</fetch>"
            ].join("");

            Xrm.WebApi.retrieveMultipleRecords(config.leaveBalanceLogicalName, "?fetchXml=" + encodeURIComponent(fetchXml))
                .then(function (result) {
                    var lookupValue = null;

                    if (result.entities.length > 0) {
                        var record = result.entities[0];
                        var recordId = LeaveRequest.Form.normalizeGuid(record[config.leaveBalanceValueField]);
                        var recordName = record[config.leaveBalanceNameField] || "";
                        lookupValue = [{ id: recordId, name: recordName, entityType: config.leaveBalanceLogicalName }];
                    }

                    LeaveRequest.Form.setLeaveBalance(formContext, lookupValue);
                })
                .catch(function (error) {
                    console.error("Error fetching leave balance: " + error.message);
                    if (formContext.ui.getFormType() === 1) {
                        LeaveRequest.Form.clearLeaveBalance(formContext);
                    }
                });
        } catch (error) {
            console.error("Error in fetchLeaveBalance: " + error.message);
        }
    },

    /**
     * Set Leave Balance on the form
     */
    setLeaveBalance: function (formContext, lookupValue) {
        try {
            var balanceAttr = formContext.getAttribute(LeaveRequest.Form.config.leaveBalanceValueField);

            if (!balanceAttr) {
                console.warn("Leave Balance field not found on form");
                return;
            }
            if (formContext.ui.getFormType() === 1) {
                balanceAttr.setValue(lookupValue || null);
            }
            console.log("Leave Balance set to: " + (lookupValue ? lookupValue[0].id : null));
        } catch (error) {
            console.error("Error setting leave balance: " + error.message);
        }
    },

    /**
     * Clear Leave Balance field value
     */
    clearLeaveBalance: function (formContext) {
        try {
            LeaveRequest.Form.setLeaveBalance(formContext, null);
        } catch (error) {
            console.error("Error clearing leave balance: " + error.message);
        }
    },

    /**
     * Normalize GUID value by removing braces
     */
    normalizeGuid: function (value) {
        return value ? value.replace(/[{}]/g, "") : value;
    },

    /**
     * Fetch Resource Record by ID and get Gender option set value
     */
    fetchResourceById: function (resourceId, formContext) {
        try {
            debugger;
            var fetchXml = [
                "<fetch>",
                "  <entity name='" + LeaveRequest.Form.config.resourceLogicalName + "'>",
                "    <attribute name='bookableresourceid' />",
                "    <attribute name='name' />",
                "    <attribute name='bcc_gender' />",
                "    <attribute name='new_type' />",
                "    <filter type='and'>",
                "      <condition attribute='bookableresourceid' operator='eq' value='" + resourceId + "' />",
                "    </filter>",
                "  </entity>",
                "</fetch>"
            ].join("");

            Xrm.WebApi.retrieveMultipleRecords(LeaveRequest.Form.config.resourceLogicalName, "?fetchXml=" + encodeURIComponent(fetchXml))
                .then(function (result) {
                    if (result.entities.length > 0) {
                        debugger;
                        var resource = result.entities[0];
                        var genderValue = resource["bcc_gender"];
                        var resourceTypeValue = resource["new_type"];
                        if (genderValue !== null && genderValue !== undefined) {
                            LeaveRequest.Form.filterLeaveTypeByGender(formContext, genderValue, resourceTypeValue);
                        }
                    }
                })
                .catch(function (error) {
                    console.error("Error fetching resource record: " + error.message);
                });
        } catch (error) {
            console.error("Error in fetchResourceById: " + error.message);
        }
    },

    /**
     * Filter Leave Type based on Gender option set value
     */
    filterLeaveTypeByGender: function (formContext, genderValue, resourceTypeValue) {
        try {
            debugger;
            var leaveTypeControl = formContext.getControl(LeaveRequest.Form.config.leaveTypeField);

            if (!leaveTypeControl) {
                console.warn("Leave Type control not found");
                return;
            }

            // Store gender and resource type values for use in filterApplied
            LeaveRequest.Form.currentGenderValue = genderValue;
            LeaveRequest.Form.currentResourceTypeValue = resourceTypeValue;

            // Add filter to the lookup control
            if (leaveTypeControl.addPreSearch) {
                if (LeaveRequest.Form.leaveTypePreSearchHandler) {
                    leaveTypeControl.removePreSearch(LeaveRequest.Form.leaveTypePreSearchHandler);
                }

                LeaveRequest.Form.leaveTypePreSearchHandler = function () {
                    LeaveRequest.Form.filterApplied(formContext, genderValue, resourceTypeValue);
                };

                leaveTypeControl.addPreSearch(LeaveRequest.Form.leaveTypePreSearchHandler);
            }

            console.log("Leave Type filter applied for gender value: " + genderValue + ", resource type value: " + resourceTypeValue);
        } catch (error) {
            console.error("Error in filterLeaveTypeByGender: " + error.message);
        }
    },

    /**
     * Apply PreSearch Filter to Leave Type Lookup using Gender option set value
     */
    filterApplied: function (formContext, genderValue, resourceTypeValue) {
        try {
            debugger;
            // Base filter for multi-select global option set (gender)
            var filterConditions = [
                "  <condition attribute='bcc_genderapplicableto' operator='contain-values'>",
                "    <value>" + genderValue + "</value>",
                "  </condition>"
            ];

            // Add resource type condition if available
            if (resourceTypeValue !== null && resourceTypeValue !== undefined) {
                filterConditions.push("  <condition attribute='bcc_resourcetype' operator='eq' value='" + resourceTypeValue + "' />");
            }

            var filterXml = ["<filter type='and'>"].concat(filterConditions).concat(["</filter>"]).join("");

            var leaveTypeControl = formContext.getControl(LeaveRequest.Form.config.leaveTypeField);
            if (leaveTypeControl) {
                leaveTypeControl.addCustomFilter(filterXml, LeaveRequest.Form.config.leaveTypeLogicalName);
            }
        } catch (error) {
            console.error("Error applying filter: " + error.message);
        }
    },

    /**
     * Clear Leave Type Filter
     */
    clearLeaveTypeFilter: function (formContext) {
        try {
            debugger;
            var leaveTypeControl = formContext.getControl(LeaveRequest.Form.config.leaveTypeField);

            if (leaveTypeControl && leaveTypeControl.removePreSearch) {
                if (LeaveRequest.Form.leaveTypePreSearchHandler) {
                    leaveTypeControl.removePreSearch(LeaveRequest.Form.leaveTypePreSearchHandler);
                    LeaveRequest.Form.leaveTypePreSearchHandler = null;
                }
            }

            formContext.getAttribute(LeaveRequest.Form.config.leaveTypeField).setValue(null);
            console.log("Leave Type filter cleared");
        } catch (error) {
            console.error("Error clearing filter: " + error.message);
        }
    },

    /**
     * Show Success Alert
     */
    showSuccessAlert: function (message) {
        try {
            let alertStrings = { confirmButtonLabel: 'Close', text: message };
            let alertOptions = { height: 120, width: 260 };

            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                function success() {
                    // Alert closed
                },
                function (error) {
                    console.log(error.message);
                }
            );
        } catch (error) {
            console.error("Error showing success alert: " + error.message);
        }
    },

    /**
     * Show Progress Indicator
     */
    showProgress: function (message) {
        try {
            Xrm.Utility.showProgressIndicator(message || "Please wait...");
        } catch (error) {
            console.error("Error showing progress: " + error.message);
        }
    },

    /**
     * Hide Progress Indicator
     */
    hideProgress: function () {
        try {
            Xrm.Utility.closeProgressIndicator();
        } catch (error) {
            console.error("Error hiding progress: " + error.message);
        }
    },

    /**
     * Show Error Alert
     */
    showErrorAlert: function (errorMessage) {
        try {
            let alertStrings = { confirmButtonLabel: 'Close', text: "Error: " + errorMessage };
            let alertOptions = { height: 120, width: 260 };

            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                function success() {
                    // Alert closed
                },
                function (error) {
                    console.log(error.message);
                }
            );
        } catch (error) {
            console.error("Error showing error alert: " + error.message);
        }
    },

    /**
     * Fetch Leave Type record and check if attachment is required
     */
    checkLeaveTypeAttachmentRequired: function (leaveTypeId, formContext) {
        try {
            var config = LeaveRequest.Form.config;
            var fetchXml = [
                "<fetch top='1'>",
                "  <entity name='" + config.leaveTypeLogicalName + "'>",
                "    <attribute name='" + config.attachmentRequiredField + "' />",
                "    <filter type='and'>",
                "      <condition attribute='bcc_leavetypeid' operator='eq' value='" + leaveTypeId + "' />",
                "    </filter>",
                "  </entity>",
                "</fetch>"
            ].join("");

            Xrm.WebApi.retrieveMultipleRecords(config.leaveTypeLogicalName, "?fetchXml=" + encodeURIComponent(fetchXml))
                .then(function (result) {
                    var attachmentRequired = false;

                    if (result.entities.length > 0) {
                        var leaveType = result.entities[0];
                        var attachmentField = leaveType[config.attachmentRequiredField];

                        // Check if attachment required is true (handle both boolean and integer values)
                        attachmentRequired = attachmentField === true || attachmentField === 1 || attachmentField === "true";
                    }

                    LeaveRequest.Form.setAttachmentFieldVisibility(formContext, attachmentRequired);
                    console.log("Attachment requirement checked: " + (attachmentRequired ? "Required" : "Not Required"));
                })
                .catch(function (error) {
                    console.error("Error fetching leave type record: " + error.message);
                    LeaveRequest.Form.setAttachmentFieldVisibility(formContext, false);
                });
        } catch (error) {
            console.error("Error in checkLeaveTypeAttachmentRequired: " + error.message);
        }
    },

    /**
     * Set Attachment Field Visibility based on requirement
     */
    setAttachmentFieldVisibility: function (formContext, isVisible) {
        try {
            var attachmentControl = formContext.getControl(LeaveRequest.Form.config.attachmentField);

            if (attachmentControl) {
                attachmentControl.setVisible(isVisible);
                // if (isVisible)
                //     formContext.getAttribute(LeaveRequest.Form.config.attachmentField).setRequiredLevel("required");
                // else {
                //     formContext.getAttribute(LeaveRequest.Form.config.attachmentField).setRequiredLevel("none");
                // }
                console.log("Attachment field visibility set to: " + (isVisible ? "Visible" : "Hidden"));
            } else {
                console.warn("Attachment control not found on form");
            }
        } catch (error) {
            console.error("Error setting attachment field visibility: " + error.message);
        }
    }
};

LeaveRequest.Ribbon = {

    onClickSendForApproval: function (formContext) {
        try {
            formContext.getAttribute("statuscode").setValue(1); // Set statuscode to 1 (Inactive) to trigger workflow
            formContext.data.entity.save().then(
                function success() {
                    LeaveRequest.Form.showSuccessAlert("Leave request sent for approval successfully.");
                },
                function (error) {
                    console.error("Error saving record: " + error.message);
                    LeaveRequest.Form.showErrorAlert("Failed to send leave request for approval. Please try again.");
                }
            );
        } catch (error) {
            console.error("Error in onClickSendForApproval: " + error.message);
        }
    }


};
