if (typeof (Project) === "undefined") {
    var Project = {};
}

if (typeof (Project.Ribbon) === "undefined") {
    Project.Ribbon = {};
}

/**
 * Open the Sub Contractor Transaction dialog modal
 */
Project.Ribbon.openSubContractorTransactionDialogModal = function(primaryControl) {
    try {
        const navigationOptions = {
            target: 2,
            height: { value: 95, unit: "%" },
            width: { value: 60, unit: "%" },
            position: 1
        };

        const pageInputs = {};

        const formContext = primaryControl;

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
