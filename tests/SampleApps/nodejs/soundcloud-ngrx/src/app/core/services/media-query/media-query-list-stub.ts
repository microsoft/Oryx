export class MediaQueryListStub {
  handlers: MediaQueryListListener[] = [];

  constructor(public media: string, public matches: boolean = false) {}

  dispatch(): void {
    this.handlers.forEach(handler => handler(this));
  }

  addListener(handler: MediaQueryListListener): void {
    this.handlers.push(handler);
  }

  removeListener(handler: MediaQueryListListener): void {
    return;
  }
}
