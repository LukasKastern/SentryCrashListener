# SentryCrashListener

This is a sample that demonstrates how you can use an external app to track Unity's crashhandler and forward crashes to Sentry.

To get started open the UnitySample project through the UnityHub.

After that navigate to Scripts/StartCrashListener, open that up and enter a testing dsn into the ```KDsn``` field.

When that is done you can build the project and run it.
You'll see a crash button which will use Unity's ForceCrash method to cause an access violation.

When you click that and everything works correctly you will get the crashlog, playerlog and dump file send as a sentry issue.
