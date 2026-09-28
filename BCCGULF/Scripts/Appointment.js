if (typeof (Appointment) === "undefined") {
    var Appointment = {};
}

if (typeof (Appointment.Form) === "undefined") {
    Appointment.Form = {};
}

Appointment.Form = {
    config: {
        regardingField: "regardingobjectid",
        requiredAttendeesField: "requiredattendees"
    },

    setRequiredAttendies: function (executionContext) {
        try {
            var formContext = executionContext.getFormContext
                ? executionContext.getFormContext()
                : executionContext;

            var regardingAttr = formContext.getAttribute(Appointment.Form.config.regardingField);
            var requiredAttendeesAttr = formContext.getAttribute(Appointment.Form.config.requiredAttendeesField);

            if (!regardingAttr || !requiredAttendeesAttr) {
                return;
            }

            var regardingValue = regardingAttr.getValue();
            if (regardingValue && regardingValue.length > 0) {
                var party = {
                    id: regardingValue[0].id,
                    entityType: regardingValue[0].entityType,
                    name: regardingValue[0].name
                };

                requiredAttendeesAttr.setValue([party]);
            }

            regardingAttr.setRequiredLevel("required");
            requiredAttendeesAttr.setRequiredLevel("required");

            if (!regardingAttr.__bccgulfRegardingRequiredHandlerAdded) {
                regardingAttr.addOnChange(Appointment.Form.onRegardingChange);
                regardingAttr.__bccgulfRegardingRequiredHandlerAdded = true;
            }
        } catch (e) {
            console.error("Appointment.Form.setRequiredAttendies - unexpected error: " + e.message);
        }
    },

    onFormLoad: function (executionContext) {
        Appointment.Form.setRequiredAttendies(executionContext);
    },

    onRegardingChange: function (executionContext) {
        Appointment.Form.setRequiredAttendies(executionContext);
    }
};
