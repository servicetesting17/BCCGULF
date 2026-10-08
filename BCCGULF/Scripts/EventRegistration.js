if (typeof (EventRegistration) === "undefined") {
    var EventRegistration = {};
}

if (typeof (EventRegistration.Form) === "undefined") {
    EventRegistration.Form = {};
}

// ✅ Event Registration Form Operations
EventRegistration.Form = {
    config: {
        stateCodeField: "statecode",
        statusCodeField: "statuscode",
        stateActive: 0,
        stateInactive: 1,

        /**
         * Status Reason options to remove, per form and per state.
         * Key   : Form Id (GUID, with or without braces, any case)
         * active   : statuscode values to remove when statecode = Active (0)
         * inactive : statuscode values to remove when statecode = Inactive (1)
         * Replace the GUIDs and option values below with the real ones.
         */
        statusOptionsToRemove: {
            "00000000-0000-0000-0000-000000000001": {   // Form 1 — update form id
                active: [100000000, 100000001],
                inactive: [100000002]
            },
            "00000000-0000-0000-0000-000000000002": {   // Form 2 — update form id
                active: [100000003],
                inactive: [100000004, 100000005]
            }
        }
    },

    /**
     * Form Load Handler — register on the form OnLoad event (pass execution context)
     */
    onFormLoad: function (executionContext) {
        try {
            var formContext = executionContext.getFormContext ? executionContext.getFormContext() : executionContext;

            EventRegistration.Form.filterStatusReasonOptions(formContext);

            var stateAttribute = formContext.getAttribute(EventRegistration.Form.config.stateCodeField);
            if (stateAttribute) {
                stateAttribute.addOnChange(EventRegistration.Form.onStateCodeChange);
            }
        } catch (e) {
            console.error("EventRegistration.Form.onFormLoad error: " + e.message);
        }
    },

    /**
     * Statecode Change Handler
     */
    onStateCodeChange: function (executionContext) {
        var formContext = executionContext.getFormContext ? executionContext.getFormContext() : executionContext;
        EventRegistration.Form.filterStatusReasonOptions(formContext);
    },

    /**
     * Removes configured statuscode options based on current form id and statecode.
     * Applies to every statuscode control on the form (body, header, etc.).
     */
    filterStatusReasonOptions: function (formContext) {
        try {
            var config = EventRegistration.Form.config;

            var currentForm = formContext.ui.formSelector.getCurrentItem();
            if (!currentForm) {
                return;
            }

            var formId = EventRegistration.Form.normalizeGuid(currentForm.getId());
            var formConfig = null;
            for (var key in config.statusOptionsToRemove) {
                if (config.statusOptionsToRemove.hasOwnProperty(key) &&
                    EventRegistration.Form.normalizeGuid(key) === formId) {
                    formConfig = config.statusOptionsToRemove[key];
                    break;
                }
            }
            if (!formConfig) {
                return;
            }

            var stateAttribute = formContext.getAttribute(config.stateCodeField);
            var statusAttribute = formContext.getAttribute(config.statusCodeField);
            if (!stateAttribute || !statusAttribute) {
                return;
            }

            var stateValue = stateAttribute.getValue();
            var optionsToRemove = [];
            if (stateValue === config.stateActive) {
                optionsToRemove = formConfig.active || [];
            } else if (stateValue === config.stateInactive) {
                optionsToRemove = formConfig.inactive || [];
            }
            if (optionsToRemove.length === 0) {
                return;
            }

            var currentStatus = statusAttribute.getValue();
            statusAttribute.controls.forEach(function (control) {
                optionsToRemove.forEach(function (optionValue) {
                    // Keep the currently selected value so the field doesn't render blank
                    if (optionValue !== currentStatus) {
                        control.removeOption(optionValue);
                    }
                });
            });
        } catch (e) {
            console.error("EventRegistration.Form.filterStatusReasonOptions error: " + e.message);
        }
    },

    normalizeGuid: function (guid) {
        return (guid || "").replace(/[{}]/g, "").toLowerCase();
    }
};
