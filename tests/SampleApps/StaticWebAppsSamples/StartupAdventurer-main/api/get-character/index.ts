import { AzureFunction, Context, HttpRequest } from "@azure/functions";

const httpTrigger: AzureFunction = async function (context: Context, req: HttpRequest, character: any): Promise<void> {
  context.log(`Getting character from id ${req.url}`);

  if (!character) {
    context.res = {
      status: 404,
    };
  } else {
    const { _etag, _rid, _self, _ts, ...char } = character;
    context.res = {
      body: char,
    };
  }
};

export default httpTrigger;
