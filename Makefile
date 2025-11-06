# Unity Mini-Game Framework Makefile

# Unity path detection (env variable > auto-detect > fallback)
ifdef UNITY_PATH
    UNITY_EXECUTABLE := $(UNITY_PATH)
else
    DETECTED_UNITY := $(shell find /Applications/Unity/Hub/Editor -name "Unity" -type f -path "*/MacOS/Unity" 2>/dev/null | head -1)
    ifdef DETECTED_UNITY
        UNITY_EXECUTABLE := $(DETECTED_UNITY)
    else
        UNITY_EXECUTABLE := /Applications/Unity/Hub/Editor/2022.3.52f1/Unity.app/Contents/MacOS/Unity
    endif
endif

PROJECT_PATH := $(shell pwd)
LOG_FILE := unity-build.log
TEST_RESULTS_DIR := TestResults

.PHONY: help test clean logs unity-info

help:
	@echo "Unity Mini-Game Framework - Available Commands:"
	@echo ""
	@echo "  make test        - Run tests"
	@echo "  make clean       - Clean build artifacts"
	@echo "  make logs        - Show build logs"
	@echo "  make unity-info  - Show Unity path"
	@echo ""
	@echo "Unity: $(UNITY_EXECUTABLE)"
	@echo ""

# Run tests
test:
	@echo "Running tests..."
	@mkdir -p $(TEST_RESULTS_DIR)
	@$(UNITY_EXECUTABLE) \
		-batchmode \
		-nographics \
		-silent-crashes \
		-logFile $(LOG_FILE) \
		-projectPath "$(PROJECT_PATH)" \
		-runTests \
		-testPlatform EditMode \
		-testResults "$(PROJECT_PATH)/$(TEST_RESULTS_DIR)/EditMode-results.xml"
	@echo "Results: $(TEST_RESULTS_DIR)/EditMode-results.xml"

# Clean build artifacts
clean:
	@echo "Cleaning..."
	@rm -rf Library/ Temp/ Logs/ $(TEST_RESULTS_DIR)
	@rm -f $(LOG_FILE) *.log
	@echo "Done"

# Show build logs
logs:
	@if [ -f "$(LOG_FILE)" ]; then \
		tail -50 $(LOG_FILE); \
	else \
		echo "No log file found"; \
	fi

# Show Unity path
unity-info:
	@echo "Unity Path: $(UNITY_EXECUTABLE)"
	@if [ -f "$(UNITY_EXECUTABLE)" ]; then \
		echo "Status: Found"; \
	else \
		echo "Status: NOT FOUND"; \
		echo ""; \
		echo "Set UNITY_PATH environment variable:"; \
		echo "  export UNITY_PATH=/path/to/Unity"; \
	fi

.DEFAULT_GOAL := help
