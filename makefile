ROOT_DIR := $(shell git rev-parse --show-toplevel)
DEV_DIR := $(ROOT_DIR)/.dev
PACKAGE := github.com/benc-uk/codex
VERSION := $(shell git describe --tags --abbrev=0 --dirty=-dev 2>/dev/null || echo "0.0.0-dev")

.DEFAULT_GOAL := help

.PHONY: help build-lua run clean install lint build-web build

help: ## Show this help message
	@echo "Available targets:"
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-15s\033[0m %s\n", $$1, $$2}'

build-lua: ## Build Lua WebAssembly binary
	GOOS=js GOARCH=wasm go build -o web/public/lua.wasm golua/main.go

run: ## Run development server
	cd web && npm run dev

serve: build ## Build and serve preview version
	cd web && npm run preview
	
clean: ## Clean build artifacts and dependencies
	rm -rf web/public/lua.wasm web/node_modules

install: ## Install dependencies for web and Go
	cd web && npm install
	go mod download

lint: ## Check code formatting and linting
	@count=$$(gofmt -l . | wc -l); \
	echo "$$count files need formatting"; \
	[ $$count -eq 0 ] || exit 1
	@cd web && npm run lint && npm run format:check

build-web: ## Build web application
	cd web && npm run build

build: build-lua build-web ## Build all components (Lua WASM + web)