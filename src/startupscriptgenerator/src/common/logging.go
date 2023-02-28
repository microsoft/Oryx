// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import (
	"common/consts"
	"fmt"
	"os"
	"strings"
	"time"

	"github.com/google/uuid"
	"github.com/microsoft/ApplicationInsights-Go/appinsights"
	"github.com/microsoft/ApplicationInsights-Go/appinsights/contracts"
)

const ShutDownCloseTimeOut time.Duration = 3 * time.Second
const ShutDownExitTimeOut time.Duration = 6 * time.Second

type Logger struct {
	AiClient    appinsights.TelemetryClient
	LoggerName  string
	AppName     string
	OperationID string
}

// Represents unique identifier that can be used to correlate messages with the build logs
var buildOpID string

func SetGlobalOperationID(buildManifest BuildManifest) {
	if buildManifest.OperationID == "" {
		// For apps which are not built by Oryx, there would not be a manifest file. So,
		// generate a unique id for those scenarios.
		fmt.Println("Could not find operation ID in manifest. Generating an operation id...")
		buildOpID = uuid.New().String()
	} else {
		buildOpID = strings.TrimSpace(buildManifest.OperationID)
	}

	logger := GetLogger("logging.SetGlobalOperationID")
	defer logger.Shutdown()

	if buildManifest.OperationID == "" {
		logger.LogInformation("No operation ID found in manifest.")
	}

	fmt.Println("Build Operation ID: " + buildOpID)
}

func GetLogger(name string) *Logger {
	key := os.Getenv(consts.ApplicationInsightsConnectionStringEnvVarName)
	logger := Logger{
		AiClient:    appinsights.NewTelemetryClient(key),
		LoggerName:  name,
		AppName:     os.Getenv(consts.AppServiceAppNameEnvVarName),
		OperationID: buildOpID,
	}
	return &logger
}

func (logger *Logger) makeTraceItem(message string, sev contracts.SeverityLevel) *appinsights.TraceTelemetry {
	trace := appinsights.NewTraceTelemetry(message, sev)
	trace.BaseTelemetry.Tags.Operation().SetId(logger.OperationID)
	trace.Properties["LoggerName"] = logger.LoggerName
	trace.Properties["AppName"] = logger.AppName
	return trace
}

func (logger *Logger) logTrace(sev contracts.SeverityLevel, format string, a ...interface{}) {
	message := fmt.Sprintf(format, a...)

	// Uncomment the follwing line to see the trace messages on standard out
	//fmt.Println(message)

	logger.AiClient.Track(logger.makeTraceItem(message, sev))
}

func (logger *Logger) LogVerbose(format string, a ...interface{}) {
	logger.logTrace(appinsights.Verbose, format, a...)
}

func (logger *Logger) LogInformation(format string, a ...interface{}) {
	logger.logTrace(appinsights.Information, format, a...)
}

func (logger *Logger) LogProperties(message string, props map[string]string) {
	trace := logger.makeTraceItem(message, appinsights.Information)
	for key, value := range props {
		// Avoids overriding pre-existing values
		if trace.Properties[key] == "" {
			trace.Properties[key] = value
		}
	}
	logger.AiClient.Track(trace)
}

func (logger *Logger) LogWarning(format string, a ...interface{}) {
	logger.logTrace(appinsights.Warning, format, a...)
}

func (logger *Logger) LogError(format string, a ...interface{}) {
	logger.logTrace(appinsights.Error, format, a...)
}

func (logger *Logger) LogCritical(format string, a ...interface{}) {
	logger.logTrace(appinsights.Critical, format, a...)
}

func (logger *Logger) Shutdown() {
	select {
	case <-logger.AiClient.Channel().Close(ShutDownCloseTimeOut):
		// Two second timeout for retries.

		// If we got here, then all telemetry was submitted
		// successfully, and we can proceed to exiting.
	case <-time.After(ShutDownExitTimeOut):
		// Five second absolute timeout.  This covers any
		// previous telemetry submission that may not have
		// completed before Close was called.

		// There are a number of reasons we could have
		// reached here.  We gave it a go, but telemetry
		// submission failed somewhere.  Perhaps old events
		// were still retrying, or perhaps we're throttled.
		// Either way, we don't want to wait around for it
		// to complete, so let's just exit.
	}
}

func (logger *Logger) StartupScriptRequested() {
	logger.LogProperties(
		"StartupScriptRequested",
		map[string]string{
			"oryxVersion":        Version,
			"oryxCommitId":       Commit,
			"oryxReleaseTagName": ReleaseTagName,
		})
}
