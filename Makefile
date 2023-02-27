
.PHONY: format
format: ## format all files
	@dotnet format --report format-report.json

.PHONY: format-info
format-info: ## format all files with 'info' severity
	@dotnet format --report format-report.json --severity info

.PHONY: format-verify-no-changes
format-verify-no-changes: ## check for formatting problems
	@dotnet format --report format-report.json --verify-no-changes

.PHONY: print-format-report
print-format-report: ## print file paths and changes from format-report.json
	@jq '.[] | "\(.FilePath)(\(.FileChanges[0].LineNumber),\(.FileChanges[0].CharNumber))","\(.FileChanges[0].FormatDescription)","---"' -r format-report.json

.PHONY: print-format-report-files
print-format-report-files: ## print file paths from format-report.json
	@jq '.[] | .FilePath + "(" + (.FileChanges[0].LineNumber|tostring) + "," + (.FileChanges[0].CharNumber|tostring) + ")"' -r format-report.json
