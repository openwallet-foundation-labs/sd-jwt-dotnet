# SD-JWT for .NET

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.svg)](https://www.nuget.org/packages/SdJwt.Net/)
[![Build Status](https://github.com/your-username/sd-jwt-net/actions/workflows/dotnet.yml/badge.svg)](https://github.com/your-username/sd-jwt-net/actions)

A production-ready, multi-platform .NET library for creating and verifying **Selectively Disclosable JSON Web Tokens (SD-JWTs)**. This SDK provides a comprehensive, secure, and easy-to-use implementation of the following IETF drafts:

- **SD-JWT Core**: `draft-ietf-oauth-selective-disclosure-jwt-22`
- **Verifiable Credentials Profile**: `draft-ietf-oauth-sd-jwt-vc-02`
- **Status List for Revocation**: `draft-ietf-oauth-status-list-02`

## Features

- **Multi-Platform**: Targets `.NET 9` and `.NET Standard 2.0` for maximum compatibility.
- **Secure by Default**: Uses constant-time comparisons, enforces strong algorithm policies, and validates all inputs.
- **Full VC Support**: Includes type-safe helpers for issuing and verifying Verifiable Credentials (`SD-JWT-VC`).
- **Revocation Ready**: Implements the Status List specification for scalable and privacy-preserving credential revocation.
- **Superior DX**: Comes with a pre-configured Dev Container for consistent, cross-platform development and a fully automated CI/CD pipeline using GitHub Actions.

## How to Use the SDK

Install the package from NuGet:
```sh
dotnet add package SdJwt.Net