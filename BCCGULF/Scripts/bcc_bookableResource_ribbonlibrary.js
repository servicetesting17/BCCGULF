function openCustomPopup(selectedItems) {
    try {
        if (!selectedItems || selectedItems.length === 0) {
            Xrm.Utility.alertDialog("Please select at least one record.");
            return;
        }

        // Collect GUIDs
        var recordIds = selectedItems.map(function (item) {
            return item.Id.replace(/[{}]/g, ""); // remove curly braces
        }).join(",");

        // Open popup (HTML Web Resource)
        var pageInput = {
            pageType: "webresource",
            webresourceName: "bcc_resourcepopup.html", // your web resource name
            data: recordIds
        };

        var navigationOptions = {
            target: 2, // open in dialog
            width: 500,
            height: 800,
            position: 1,
            title: "BCCGulf Time Entries"
        };

        Xrm.Navigation.navigateTo(pageInput, navigationOptions).then(
            function () {
                console.log("Popup closed");
            },
            function (error) {
                console.error(error);
            }
        );
    } catch (e) {
        console.error(e);
    }
}

function openDeactivatepopup(selectedItems) {
    try {
        if (!selectedItems || selectedItems.length === 0) {
            Xrm.Utility.alertDialog("Please select at least one record.");
            return;
        }

        // Get the first selected record ID
        var recordId = selectedItems[0].Id.replace(/[{}]/g, "");

        // Fetch related IT Assets
        fetchRelatedAssets(recordId, function (assetsData) {
            // Open popup with assets data
            var pageInput = {
                pageType: "webresource",
                webresourceName: "bcc_deactivaterecorddatecapturing",
                data: JSON.stringify({
                    recordId: recordId,
                    assets: assetsData
                })
            };

            var navigationOptions = {
                target: 2,
                width: 800,
                height: 900,
                position: 1,
                title: "Deactivate Resource"
            };

            Xrm.Navigation.navigateTo(pageInput, navigationOptions).then(
                function () {
                    console.log("Popup closed");
                },
                function (error) {
                    console.error(error);
                    Xrm.Utility.alertDialog("Error opening popup: " + error.message);
                }
            );
        });

    } catch (e) {
        console.error(e);
        Xrm.Utility.alertDialog("Error: " + e.message);
    }
}

function openDeactivatepopupp() {
    try {
        // Get the current record ID from the form context
        var currentRecordId = null;

        if (Xrm && Xrm.Page && Xrm.Page.data && Xrm.Page.data.entity) {
            currentRecordId = Xrm.Page.data.entity.getId();
            // Remove curly braces if present
            currentRecordId = currentRecordId.replace(/[{}]/g, "");
        }

        if (!currentRecordId) {
            Xrm.Utility.alertDialog("Unable to get current record ID.");
            return;
        }

        // Fetch related IT Assets
        fetchRelatedAssets(currentRecordId, function (assetsData) {
            // Open popup with assets data
            var pageInput = {
                pageType: "webresource",
                webresourceName: "bcc_deactivateresourceonform",
                data: JSON.stringify({
                    recordId: currentRecordId,
                    assets: assetsData
                })
            };

            var navigationOptions = {
                target: 2,
                width: 800,
                height: 900,
                position: 1,
                title: "Deactivate Resource"
            };

            Xrm.Navigation.navigateTo(pageInput, navigationOptions).then(
                function () {
                    console.log("Popup closed");
                    // Refresh the form after popup closes
                    if (Xrm.Page.data && Xrm.Page.data.refresh) {
                        Xrm.Page.data.refresh(false);
                    }
                },
                function (error) {
                    console.error(error);
                    Xrm.Utility.alertDialog("Error opening popup: " + error.message);
                }
            );
        });

    } catch (e) {
        console.error(e);
        Xrm.Utility.alertDialog("Error: " + e.message);
    }
}

// Helper function to fetch related IT Assets
function fetchRelatedAssets(bookableResourceId, callback) {
    try {
        // FetchXML to get related IT Assets with additional fields
        var fetchXml = [
            "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>",
            "  <entity name='bcc_itasset'>",
            "    <attribute name='bcc_itassetid' />",
            "    <attribute name='bcc_assettype' />",
            "    <attribute name='bcc_itasset1' />",
            "    <attribute name='bcc_modelno' />",
            "    <attribute name='bcc_serialnumber' />",
            "    <attribute name='new_mobile' />",
            "    <filter type='and'>",
            "      <condition attribute='new_employee' operator='eq' value='{" + bookableResourceId + "}' />",
            "      <condition attribute='statecode' operator='eq' value='0' />",
            "    </filter>",
            "  </entity>",
            "</fetch>"
        ].join("");

        // Encode fetchXml
        var encodedFetchXml = encodeURIComponent(fetchXml);

        // Execute the query
        Xrm.WebApi.retrieveMultipleRecords("bcc_itasset", "?fetchXml=" + encodedFetchXml).then(
            function success(result) {
                var assetsData = [];

                for (var i = 0; i < result.entities.length; i++) {
                    var asset = result.entities[i];
                    var assetTypeValue = asset["bcc_assettype"];
                    var assetTypeName = "";

                    // Map option set values to names
                    switch (assetTypeValue) {
                        case 755700000:
                            assetTypeName = "Laptop";
                            break;
                        case 755700001:
                            assetTypeName = "PC";
                            break;
                        case 755700002:
                            assetTypeName = "SIM";
                            break;
                        case 755700003:
                            assetTypeName = "Accessories";
                            break;
                        default:
                            assetTypeName = "Unknown";
                    }

                    assetsData.push({
                        id: asset["bcc_itassetid"],
                        assetType: assetTypeValue,
                        assetTypeName: assetTypeName,
                        assetName: asset["bcc_itasset1"] || "N/A",
                        modelNo: asset["bcc_modelno"] || "N/A",
                        serialNumber: asset["bcc_serialnumber"] || "N/A",
                        mobilenumber: asset["new_mobile"] || "N/A"
                    });
                }

                callback(assetsData);
            },
            function (error) {
                console.error("Error fetching assets: ", error.message);
                Xrm.Utility.alertDialog("Error fetching related assets: " + error.message);
                callback([]);
            }
        );

    } catch (e) {
        console.error("Error in fetchRelatedAssets: ", e);
        callback([]);
    }
}

