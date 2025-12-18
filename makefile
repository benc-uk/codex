LOCAL_PORT ?= 8000
CONFIG ?= Debug
.DEFAULT_GOAL := help

.PHONY: help testharness install

help: # 💬 Show this help message
	@grep -E '^[a-zA-Z_-]+:.*?# .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?# "}; {printf "  \033[36m%-15s\033[0m %s\n", $$1, $$2}'

run: # 🧪 Run test harness
	dotnet run --project console/console.csproj -- ./test/demo-story.yaml

run-web: # 🌐 Run web frontend
	dotnet watch --project frontend/frontend.csproj

build-web: # 🏗️ Build web frontend
	dotnet publish frontend/frontend.csproj -c $(CONFIG)

install: # 📦 Install dependencies
	dotnet tool install dotnet-serve
	dotnet restore codex.slnx