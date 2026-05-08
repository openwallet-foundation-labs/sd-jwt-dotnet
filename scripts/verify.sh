#!/usr/bin/env bash
set -euo pipefail

SOLUTION="${1:-SdJwt.Net.sln}"
CONFIGURATION="${CONFIGURATION:-Release}"
SKIP_FORMAT="${SKIP_FORMAT:-false}"
SKIP_VULNERABILITY_SCAN="${SKIP_VULNERABILITY_SCAN:-false}"
DOTNET_STABILITY_ARGS=(
  "-m:1"
  "-p:BuildInParallel=false"
  "-p:UseSharedCompilation=false"
  "-p:GeneratePackageOnBuild=false"
)

step() {
  local name="$1"
  echo
  echo "==> ${name}"
}

step "Restore dependencies"
dotnet restore "${SOLUTION}"

step "Build solution (${CONFIGURATION})"
while IFS= read -r project; do
  echo "Building ${project}"
  dotnet build "${project}" --configuration "${CONFIGURATION}" --no-restore "${DOTNET_STABILITY_ARGS[@]}"
done < <(dotnet sln "${SOLUTION}" list | grep -E '\.csproj$' | sort)

step "Run tests"
while IFS= read -r test_project; do
  echo "Testing ${test_project}"
  dotnet test "${test_project}" --configuration "${CONFIGURATION}" --no-build --framework net10.0 --verbosity normal "${DOTNET_STABILITY_ARGS[@]}"
done < <(dotnet sln "${SOLUTION}" list | grep -E '^tests[\\/].*\.csproj$' | sort)

if [[ "${SKIP_FORMAT}" != "true" ]]; then
  step "Verify code formatting"
  if ! dotnet format --verify-no-changes --verbosity normal "${SOLUTION}"; then
    echo ""
    echo ".NET code formatting issues detected!"
    echo ""
    echo "To fix locally, run:"
    echo "  dotnet format ${SOLUTION}"
    echo ""
    echo "Or commit with the pre-commit hook enabled (it auto-formats)."
    exit 1
  fi
  echo ".NET code is properly formatted."

  step "Verify Markdown formatting"
  file_count=$(find . -name '*.md' -not -path './node_modules/*' -not -path './.git/*' -type f 2>/dev/null | wc -l | tr -d ' ')
  echo "Checking ${file_count} Markdown files..."
  if ! find . -name '*.md' -not -path './node_modules/*' -not -path './.git/*' -type f -print0 2>/dev/null | xargs -0 npx --yes prettier@3.2.5 --check; then
    echo ""
    echo "Markdown formatting issues detected!"
    echo ""
    echo "To fix locally, run:"
    echo "  npx prettier@3.2.5 --write \"**/*.md\""
    echo ""
    echo "Or commit with the pre-commit hook enabled (it auto-formats)."
    exit 1
  fi
  echo "All Markdown files are properly formatted."
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
