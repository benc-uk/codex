LOCAL_PORT ?= 8000
CONFIG ?= Debug
.DEFAULT_GOAL := help

.PHONY: help testharness install

help: # 💬 Show this help message
	@grep -E '^[a-zA-Z_-]+:.*?# .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?# "}; {printf "  \033[36m%-15s\033[0m %s\n", $$1, $$2}'

testharness: # 🧪 Run test harness
	dotnet watch --project testharness/testharness.csproj -- ./test/demo-story.yaml

install: # 📦 Install dependencies
	dotnet tool install dotnet-serve
	dotnet restore codex.slnx