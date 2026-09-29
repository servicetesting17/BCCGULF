var BCCJobOffer = BCCJobOffer || {};

BCCJobOffer.Form = {
    config: {
        airTicketField: "bcc_airticket",
        airTicketAttachmentField: "bcc_attatchairticket"
    },

    onLoad: function (executionContext) {
        var formContext = executionContext.getFormContext();
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
