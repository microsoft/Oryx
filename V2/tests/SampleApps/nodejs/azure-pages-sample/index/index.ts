import { AzureFunction, Context, HttpRequest } from "@azure/functions";

const index: AzureFunction = async function (context: Context, req: HttpRequest): Promise<void> {
    context.log('HTTP trigger function processed a request.');
    context.res = {
        status: 200,
        body: {
            message: "Hellow Azure Pages"
        }
    };
};

export default index;
