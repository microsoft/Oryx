export class Config {
	get eventID() {
		return this._eventIDFromStorage() || "unknown";
	}

	persistEventID() {
		let params = new URLSearchParams(window.location.search);
		let eventParam = params.get("eventID") as string;
		if (eventParam) {
			this._writeEventIDToStorage(eventParam);
		}
	}

	// TODO: I fully realise that this should be probably be pulled into Redux. Right now the likely race conditions are
	// limited and my patience for dealing with redux boilerplate is even more limited.
	_eventIDFromStorage() {
		return window.localStorage.getItem("StartupAdventurer_eventID");
	}

	_writeEventIDToStorage(eventID: string) {
		window.localStorage.setItem("StartupAdventurer_eventID", eventID);
	}
}
