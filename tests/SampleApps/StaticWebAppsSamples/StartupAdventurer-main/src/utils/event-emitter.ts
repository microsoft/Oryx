import noop from "lodash-es/noop";

interface IEventEmitter {
	events: { [key: string]: typeof noop[] };
	emit: typeof noop;
	subscribe: typeof noop;
}

const EventEmitter: IEventEmitter = {
	events: {},
	emit: function(event: string, data: any) {
		if (!this.events[event]) return;
		this.events[event].forEach((callback: typeof noop) => callback(data));
	},
	subscribe: function(event: string, callback: typeof noop) {
		if (!this.events[event]) this.events[event] = [];
		this.events[event].push(callback);
	},
};

export default EventEmitter;
