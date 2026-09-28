if (typeof (Payslip) === "undefined") {
    var Payslip = {};
}

if (typeof (Payslip.Form) === "undefined") {
    Payslip.Form = {};
}

Payslip.Form = {
    config: {
        entityName: "bcc_payslip",
        resourceEntityName: "bookableresource",
        resourceField: "bcc_bookableresourceid",
        resourceTypeField: "new_type",
        staffTypeValue: 100000001,
        staffFormId: "4a32c8d5-4a6e-f111-ab0f-70a8a5200365" ,
        workerTypeValue: 100000000,
        workerFormId: "11109379-0b0b-4a3e-b67b-6ddef20db434" 
    },

    onFormLoad: function (executionContext) {
        try {
            var formContext = executionContext.getFormContext
                ? executionContext.getFormContext()
                : executionContext;

            // Skip on Create — no record data yet, nothing to switch on
            if (formContext.ui.getFormType() === 1) {
                return;
            }

            var resourceAttr = formContext.getAttribute(Payslip.Form.config.resourceField);
            if (!resourceAttr) {
                return;
            }

            var resourceLookup = resourceAttr.getValue();
            if (!resourceLookup || resourceLookup.length === 0) {
                return;
            }

            var resourceId = resourceLookup[0].id.replace(/[{}]/g, "");

            Xrm.WebApi.retrieveRecord(
                Payslip.Form.config.resourceEntityName,
                resourceId,
                "?$select=" + Payslip.Form.config.resourceTypeField
            ).then(
                function (record) {
                    var resourceType = record[Payslip.Form.config.resourceTypeField];
                    var cfg = Payslip.Form.config;
                    var targetFormId = null;

                    if (resourceType === cfg.staffTypeValue) {
                        targetFormId = cfg.staffFormId;
                    } else if (resourceType === cfg.workerTypeValue) {
                        targetFormId = cfg.workerFormId;
                    }

                    if (targetFormId) {
                        Payslip.Form.switchToForm(formContext, targetFormId);
                    }
                },
                function (error) {
                    console.error("Payslip.Form.onFormLoad - failed to retrieve bookable resource: " + error.message);
                }
            );
        } catch (e) {
            console.error("Payslip.Form.onFormLoad - unexpected error: " + e.message);
        }
    },

    switchToForm: function (formContext, targetFormId) {
        try {
            var normalizedTarget = targetFormId.replace(/[{}]/g, "").toLowerCase();

            var currentItem = formContext.ui.formSelector.getCurrentItem();
            if (currentItem) {
                var currentId = currentItem.getId().replace(/[{}]/g, "").toLowerCase();
                if (currentId === normalizedTarget) {
                    // Already on the correct form — nothing to do
                    return;
                }
            }

            var formItems = formContext.ui.formSelector.items.get();
            for (var i = 0; i < formItems.length; i++) {
                var itemId = formItems[i].getId().replace(/[{}]/g, "").toLowerCase();
                if (itemId === normalizedTarget) {
                    formItems[i].navigate();
                    return;
                }
            }

            console.warn("Payslip.Form.switchToForm - target form not found in selector: " + targetFormId);
        } catch (e) {
            console.error("Payslip.Form.switchToForm - unexpected error: " + e.message);
        }
    }
};
