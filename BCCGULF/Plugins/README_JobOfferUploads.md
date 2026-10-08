# JobOfferUploads Plugin - Multi-File Type Handler

## Overview
The `JobOfferUploads` plugin automatically detects when files are uploaded to JobOffer records and sends them to the Yovan ERP API with the appropriate payload format for each file type.

## Request to Proceed Webhook

When `bcc_requesttoproceed` changes from `false` to `true`, the plugin sends a
POST request to `https://eportal.yovant.ae/api/bcc/webhook/request-to-proceed`.
The webhook uses the `X-API-Key` header and sends this payload:

```json
{
  "passport_no": "AR111951",
  "status": "request_to_proceed",
  "status_date": "2026-09-29T10:30:00",
  "bcc_reference": "BCC-PRO-000123"
}
```

`passport_no` is read from `bcc_passportno`, `bcc_reference` from
`bcc_jobapplicationnumber`, and `status_date` is the current UTC time formatted
as `yyyy-MM-ddTHH:mm:ss`.

The webhook response must contain `"success": true` for the plugin to continue.
A false or missing `success` value raises a plugin error. To display that error
to the user in the model-driven app, the plugin step must run synchronously;
an asynchronous step can only report the failure in the system job.

The separate `Request_Proceed` call must also return `"state_changed": true`
(for example, `{"job_offer_state":"submitted","state_changed":true}`). If
`state_changed` is false, the plugin raises an error.

## Entry Permit Application API

When either `bcc_entrypermitapplication` or `bcc_ereceipt` is first uploaded,
the plugin posts to
`https://eportal.yovant.ae/api/bcc/webhook/entry-permit-application`.
Both files must be present because the API requires both base64 values. The
payload includes the passport number from `bcc_passportno` and the application
number from `bcc_jobapplicationnumber`. Optional API properties are omitted.

Add `bcc_entrypermitapplication`, `bcc_ereceipt`,
`bcc_passportno`, and `bcc_jobapplicationnumber` to the plugin step's PostImage.
Include the file attributes in the PreImage as well to detect their first upload.

## Supported File Types

### 1. **MohreLetter** (MOHRE Offer Letter)
**Endpoint:** `https://www.yovanterp.com/api/bcc/webhook/mohre-offer`

**Payload Format:**
```json
{
	"passport_no": "Y1850639",
	"file_base64": "JVBERi0xLjQK...base64-encoded-pdf...",
	"filename": "mohre_offer.pdf",
	"employment_contract_base64": "JVBERi0xLjQK...base64-encoded-ec-pdf...",
	"st_transaction_number": "ST123456",
	"mb_transaction_number": "MB789012"
}
```

**Required Fields in JobOffer:**
- `bcc_passportno` - Passport number
- `bcc_mohreletter` - MOHRE letter file
- `bcc_employeecontract` - Employment contract (auto-included if present)
- Transaction numbers (map to appropriate attributes)

---

### 2. **PreApprovalWorkPermit** (Entry Permit)
**Endpoint:** `https://www.yovanterp.com/api/bcc/webhook/entry-permit`

**Payload Format:**
```json
{
	"passport_no": "Y1850639",
	"file_base64": "JVBERi0xLjQK...base64-encoded-pdf...",
	"filename": "entry_permit.pdf",
	"entry_permit_number": "201/2026/12345",
	"date_of_issue": "2026-05-11",
	"uid_no": "U1234567"
}
```

**Required Fields in JobOffer:**
- `bcc_passportno` - Passport number
- `bcc_preapprovalworkpermit` - Entry permit file
- Associated permit/visa/UID attributes

---

### 3. **EmployeeContract**
**Endpoint:** `https://www.yovanterp.com/api/bcc/webhook/employment-contract`

**Payload Format:**
```json
{
	"passport_no": "Y1850639",
	"file_base64": "JVBERi0xLjQK...base64-encoded-pdf...",
	"filename": "contract.pdf"
}
```

---

### 4. **JobOffer** (Company Offer Letter)
**Endpoint:** `https://www.yovanterp.com/api/bcc/webhook/company-offer`

**Payload Format:**
```json
{
	"passport_no": "Y1850639",
	"file_base64": "JVBERi0xLjQK...base64-encoded-pdf...",
	"filename": "company_offer.pdf"
}
```

---

## How It Works

### File Download Process
The plugin uses the **InitializeFileBlocksDownloadRequest** and **DownloadBlockRequest** API for secure file retrieval:

1. **Initialize Download:**
   ```csharp
   var initializeRequest = new InitializeFileBlocksDownloadRequest
   {
       Target = new EntityReference(entityName, recordId),
       FileAttributeName = "file_column_name"
   };
   var response = (InitializeFileBlocksDownloadResponse)service.Execute(initializeRequest);
   ```
   Returns: FileContinuationToken and FileSizeInBytes

2. **Download Blocks:**
   - Downloads file in **4MB blocks** (maximum block size)
   - Handles large files by downloading sequentially
   - Each block requires the continuation token, offset, and block length

3. **Assemble Complete File:**
   - Combines all blocks into a single byte array
   - Properly handles final block (which may be smaller than 4MB)

### Detection Logic
1. **PreImage vs PostImage Comparison:**
   - Checks if file was NOT in PreImage
   - Checks if file IS in PostImage
   - This ensures only newly uploaded files are processed (not updates to other fields)

2. **File Type Identification:**
   - Iterates through supported file columns
   - When a new file is detected, identifies the file type
   - Routes to the appropriate payload builder

### API Call Flow
1. Extract file content as byte array
2. Get PassportNo from record
3. Get filename from the `{column_name}_name` attribute
4. Build dynamic payload based on file type
5. POST to appropriate API endpoint with `X-API-Key` header
6. Return response

---

## Plugin Registration

**Entity:** `bcc_joboffer` (JobOffer)
**Message:** Update
**Stage:** Post-operation (Asynchronous - Recommended)
**Execution Order:** Any

### Images Required:
Register with BOTH PreImage and PostImage using alias: **"Image"**

**Attributes to Include:**
- `bcc_requesttoproceed`
- `bcc_jobapplicationnumber` (For request-to-proceed webhook reference)
- `bcc_passportno`
- `bcc_joboffer` (JobOffer file)
- `bcc_joboffer_name`
- `bcc_employeecontract` (Employee Contract file)
- `bcc_employeecontract_name`
- `bcc_preapprovalworkpermit` (Pre-approval Work Permit file)
- `bcc_preapprovalworkpermit_name`
- `bcc_mohreletter` (MOHRE Letter file)
- `bcc_mohreletter_name`
- `bcc_transactionid` (For MOHRE letter)
- `bcc_molnumber` (For MOHRE letter)
- `bcc_visano` (For Entry Permit)
- `bcc_passportissuedate` (For Entry Permit)
- `bcc_uidno` (For Entry Permit)

---

## Customization Guide

### Adding a New File Type
1. Add the new file column name to the `ColumnsNames` array in `Execute()` method
2. Add a new case in the switch statement to determine the API URL
3. Create a new payload builder method (e.g., `BuildNewFileTypePayload()`)
4. Add a new data contract class for the payload
5. Add necessary attribute mappings in the `BuildDynamicPayload()` method

### Modifying Attribute Mappings
If your JobOffer entity uses different attribute names, update these lines:

**For MohreLetter:**
```csharp
st_transaction_number = GetAttributeValue(postImage, "bcc_transactionid", tracingService),
mb_transaction_number = GetAttributeValue(postImage, "bcc_molnumber", tracingService)
```

**For PreApprovalWorkPermit:**
```csharp
entry_permit_number = GetAttributeValue(postImage, "bcc_visano", tracingService),
date_of_issue = GetDateAttributeValue(postImage, "bcc_passportissuedate", tracingService),
uid_no = GetAttributeValue(postImage, "bcc_uidno", tracingService)
```

---

## Configuration

### API Key
Located in the constant:
```csharp
private const string API_KEY = "cc782227-a72c-40e0-97e4-6e7771f2d0c1";
```

### Base API URL
Located in the constant:
```csharp
private const string BASE_API_URL = "https://www.yovanterp.com/api/bcc/webhook/";
```

Endpoints are dynamically constructed as: `BASE_API_URL + {endpoint_suffix}`

---

## Error Handling

- **Missing PreImage/PostImage:** Plugin exits gracefully if images aren't provided
- **Missing File Content:** Skips API call if file is empty
- **Missing PassportNo:** Skips API call if passport number is not set
- **API Failures:** Throws `InvalidPluginExecutionException` with detailed error message
- **All operations logged:** Comprehensive tracing for debugging

---

## Tracing & Debugging

The plugin includes extensive tracing:
- File type detection
- PreImage/PostImage comparison results
- File content size
- Payload size
- API request/response details
- All errors and exceptions

Enable plugin tracing in Plugin Registration Tool to see detailed execution logs.

---

## Performance Notes

- **Asynchronous execution recommended** for file uploads (can be large)
- **Block-based download:** Files are downloaded in 4MB blocks for optimal performance
- **Memory efficient:** Uses MemoryStream to combine blocks without loading entire file into memory at once
- **Large file support:** Properly handles files larger than 4MB
- **Single file per update** - Plugin processes only one file upload per trigger
- **Batch operations:** Each file upload triggers the plugin individually

---

## Testing

### Test Scenario 1: New File Upload
1. Create JobOffer record with PassportNo
2. Upload a file to `bcc_mohreletter`
3. Verify API call is made with correct payload

### Test Scenario 2: File Update (Not New)
1. Create JobOffer with file already uploaded
2. Update PassportNo or other field
3. Verify API is NOT called (file already existed)

### Test Scenario 3: Multiple File Types
1. Upload different files to different columns
2. Verify each triggers separate API call with correct endpoint

---

## Support & Troubleshooting

### Plugin Not Triggering
- Verify entity name is correct: `bcc_joboffer`
- Check that message is registered for: `Update`
- Confirm stage is: `Post-operation`
- Verify `Image` alias is used for PreImage and PostImage

### Incorrect Payload
- Check attribute name mappings in payload builder methods
- Verify JobOffer record has all required attributes populated
- Review tracing logs for attribute retrieval errors

### API Call Failures
- Verify API endpoint is accessible and API key is correct
- Check network/firewall permissions for D365 → ERP API
- Review API error response in tracing logs
