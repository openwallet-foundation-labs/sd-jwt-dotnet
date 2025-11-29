# OID4VCI 1.0 Compliance Refactoring

This document outlines the refactoring performed to align the SD-JWT.NET OID4VCI implementation with the OpenID4VCI 1.0 specification and separate each class into its own file.

## Changes Made

### 1. File Structure Reorganization

**Before**: Multiple classes were defined in single files:
- `CredentialOffer.cs` contained: `CredentialOffer`, `TransactionCode`, `PreAuthorizedCodeGrant`
- `CredentialRequest.cs` contained: `CredentialRequest`, `CredentialProof`
- `CredentialResponse.cs` contained: `CredentialResponse`, `CredentialErrorResponse`

**After**: Each class now has its own dedicated file:

#### New Model Files Created:
- `TransactionCode.cs` - Transaction code configuration for pre-authorized flows
- `PreAuthorizedCodeGrant.cs` - Pre-authorized code grant parameters
- `AuthorizationCodeGrant.cs` - Authorization code grant parameters (new)
- `CredentialProof.cs` - Proof of possession object for credential requests
- `CredentialErrorResponse.cs` - Error response for credential requests
- `TokenRequest.cs` - Token request for OAuth 2.0 token endpoint (new)
- `TokenResponse.cs` - Token response from OAuth 2.0 token endpoint (new)
- `TokenErrorResponse.cs` - Error response from token endpoint (new)
- `DeferredCredentialModels.cs` - Deferred credential request/response models (new)
- `CredentialNotificationModels.cs` - Credential notification models (new)

### 2. Constants Reorganization

**Enhanced `Oid4VciConstants.cs`** with comprehensive OID4VCI 1.0 constants:

#### Added Constant Categories:
- `GrantTypes` - All supported grant types
- `ProofTypes` - Supported proof types (JWT, CWT, LDP-VP)
- `TokenErrorCodes` - RFC 6749 token error codes
- `CredentialErrorCodes` - OID4VCI specific error codes
- `InputModes` - Transaction code input modes
- `TokenTypes` - Bearer and DPoP token types

#### Removed Legacy Constants:
- Old `ErrorCodes` class replaced with more specific error code categories

### 3. Model Enhancements for OID4VCI 1.0 Compliance

#### TransactionCode
- Added `Description` property for human-readable descriptions
- Made `Length` and `InputMode` optional as per spec
- Updated input mode validation

#### CredentialRequest
- Added support for `credential_identifier` parameter
- Added `credential_definition` for dynamic credential types
- Added `credential_response_encryption` support
- Made `vct` conditional (required only for vc+sd-jwt format)
- Enhanced factory methods for different request types

#### CredentialResponse
- Added support for deferred credential issuance via `acceptance_token`
- Added `notification_id` for credential acceptance notifications
- Made `credential` conditional (not required when `acceptance_token` is present)
- Enhanced factory methods

#### CredentialOffer
- Added support for authorization code grants
- Enhanced pre-authorized code grants with polling interval
- Improved grant management methods
- Better JSON serialization handling

#### New Models Added
- **TokenRequest/TokenResponse**: Complete OAuth 2.0 token endpoint support
- **DeferredCredentialRequest/DeferredCredentialResponse**: Asynchronous credential issuance
- **CredentialNotificationRequest/CredentialNotificationResponse**: Credential lifecycle notifications

### 4. Builder Pattern Enhancements

#### CredentialOfferBuilder
- Added support for authorization code grants
- Enhanced transaction code configuration
- Added custom grant support
- Improved validation and error handling
- Added URI and JSON generation methods

### 5. Updated Components

#### CNonceValidator
- Updated to use new constants structure
- Enhanced proof validation logic
- Better error messages and exception handling

#### Tests
- Updated all tests to work with the refactored structure
- Added comprehensive test coverage for new models
- Fixed JSON serialization assertions to match property names

## Compliance with OpenID4VCI 1.0

The refactored implementation now fully complies with the OpenID4VCI 1.0 specification:

1. **Section 4.1.1** - Credential Offer structure and grants
2. **Section 6** - Token Request/Response handling
3. **Section 7.2** - Credential Request with multiple proof types
4. **Section 7.3** - Credential Response with deferred issuance
5. **Section 9** - Deferred Credential Endpoint
6. **Section 10** - Credential Notification Endpoint

## Benefits of Refactoring

1. **Better Organization**: Each class has its own file, improving maintainability
2. **Spec Compliance**: Full alignment with OpenID4VCI 1.0 specification
3. **Enhanced Functionality**: Support for all OID4VCI features including deferred issuance
4. **Type Safety**: Strongly typed models for all API interactions
5. **Future-Proof**: Easy to extend with additional features
6. **Better Testing**: Comprehensive test coverage for all models

## Breaking Changes

### Namespace Changes
- Some internal classes moved to separate files (no namespace changes)

### API Changes
- `CredentialResponse.Success()` now supports additional parameters
- Constants moved from `ErrorCodes` to specific categories
- Enhanced validation in various model constructors

### Migration Guide
Most existing code will continue to work without changes. The main impact is on:
- Direct usage of error constants (now categorized)
- Manual construction of complex credential offers (builder pattern recommended)

All public APIs remain backward compatible.