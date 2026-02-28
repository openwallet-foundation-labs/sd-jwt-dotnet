#!/usr/bin/env bash
set -euo pipefail

SOLUTION="${1:-SdJwt.Net.sln}"
CONFIGURATION="${CONFIGURATION:-Release}"
SKIP_FORMAT="${SKIP_FORMAT:-false}"
SKIP_VULNERABILITY_SCAN="${SKIP_VULNERABILITY_SCAN:-false}"

step() {
  local name="$1"
  echo
  echo "==> ${name}"
}

step "Restore dependencies"
dotnet restore "${SOLUTION}"

step "Build solution (${CONFIGURATION})"
dotnet build "${SOLUTION}" --configuration "${CONFIGURATION}" --no-restore

step "Run tests"
dotnet test "${SOLUTION}" --configuration "${CONFIGURATION}" --no-build --verbosity normal

if [[ "${SKIP_FORMAT}" != "true" ]]; then
  step "Verify code formatting"
  dotnet format --verify-no-changes --verbosity normal "${SOLUTION}"

  step "Verify Markdown formatting"
  changed_md=$(git diff --name-only HEAD~1 HEAD -- '*.md' 2>/dev/null || git diff --cached --name-only -- '*.md' 2>/dev/null || true)
  if [[ -n "${changed_md}" ]]; then
    existing_files=()
    while IFS= read -r file; do
      [[ -f "${file}" ]] && existing_files+=("${file}")
    done <<< "${changed_md}"
    if [[ ${#existing_files[@]} -gt 0 ]]; then
      echo "Checking: ${existing_files[*]}"
      npx --yes prettier@3.2.5 --check "${existing_files[@]}"
    fi
  else
    echo "No changed Markdown files to check"
  fi
fi

if [[ "${SKIP_VULNERABILITY_SCAN}" != "true" ]]; then
  step "Scan for vulnerable packages"
  vulnerability_report="$(mktemp)"
  dotnet list "${SOLUTION}" package --vulnerable --include-transitive | tee "${vulnerability_report}"

  if grep -q "has the following vulnerable packages" "${vulnerability_report}"; then
    echo
    echo "ERROR: Vulnerable packages detected."
    exit 1
  fi
fi

echo
echo "Verification completed successfully."
