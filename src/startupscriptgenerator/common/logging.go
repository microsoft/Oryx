package common

import (
	"os"
	"fmt"
	"time"
	"github.com/Microsoft/ApplicationInsights-Go/appinsights"
	"github.com/Microsoft/ApplicationInsights-Go/appinsights/contracts"
)

const SHUTDOWN_CLOSE_TIMEOUT time.Duration = 3 * time.Second
const SHUTDOWN_EXIT_TIMEOUT time.Duration  = 6 * time.Second
const APPLICATION_INSIGHTS_INSTRUMENTATION_KEY_ENV_VAR_NAME string = "ORYX_AI_INSTRUMENTATION_KEY"
const APP_SERVICE_APP_NAME_ENV_VAR_NAME string = "APPSETTING_WEBSITE_SITE_NAME"

type Logger struct {
	AiClient	appinsights.TelemetryClient
	LoggerName	string
	AppName		string
}

func GetLogger(name string) *Logger {
	key := os.Getenv(APPLICATION_INSIGHTS_INSTRUMENTATION_KEY_ENV_VAR_NAME)
	logger := Logger{
		AiClient:	appinsights.NewTelemetryClient(key),
		LoggerName:	name,
		AppName:	os.Getenv(APP_SERVICE_APP_NAME_ENV_VAR_NAME),
	}
	return &logger
}

func (logger *Logger) makeTraceItem(message string, sev contracts.SeverityLevel) *appinsights.TraceTelemetry {
	trace := appinsights.NewTraceTelemetry(message, sev)
	trace.Properties["LoggerName"] = logger.LoggerName
	trace.Properties["AppName"] = logger.AppName
	return trace
}

func (logger *Logger) logTrace(sev contracts.SeverityLevel, format string, a ...interface{}) {
	logger.AiClient.Track(logger.makeTraceItem(fmt.Sprintf(format, a...), sev))
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
	case <-logger.AiClient.Channel().Close(SHUTDOWN_CLOSE_TIMEOUT):
		// Two second timeout for retries.
		
		// If we got here, then all telemetry was submitted
		// successfully, and we can proceed to exiting.
	case <-time.After(SHUTDOWN_EXIT_TIMEOUT):
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