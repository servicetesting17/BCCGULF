if (typeof (SubContractor) === "undefined") {
    var SubContractor = {};
}

if (typeof (SubContractor.Ribbon) === "undefined") {
    SubContractor.Ribbon = {};
}

/**
 * Open the Sub Contractor Transaction dialog modal
 */
SubContractor.Ribbon.openSubContractorTransactionDialogModal = function(primaryControl) {
    try {
        const navigationOptions = {
            target: 2,
            height: { value: 900, unit: "px" },
            width: { value: 700, unit: "px" },
            position: 1
        };

        const pageInputs = {};

        const formContext = primaryControl && typeof primaryControl.getAttribute === "function"
            ? primaryControl
            : (typeof Xrm !== "undefined" && Xrm.Page && Xrm.Page.data ? Xrm.Page : null);

        if (formContext && formContext.data && formContext.data.entity) {
            const entity = formContext.data.entity;
            const entityId = entity.getId ? entity.getId() : "";
            const projectId = entityId ? entityId.replace(/[{}]/g, "") : "";

            if (projectId) {
                pageInputs.projectId = projectId;
            }
        }

        Xrm.Navigation.navigateTo({
            pageType: "webresource",
            webresourceName: "bcc_SubContractorTransaction",
            data: encodeURIComponent(JSON.stringify(pageInputs))
        }, navigationOptions);

    } catch (error) {
        console.error("Error:", error);
        Xrm.Utility.alertDialog("Error: " + error.message);
    }
};
