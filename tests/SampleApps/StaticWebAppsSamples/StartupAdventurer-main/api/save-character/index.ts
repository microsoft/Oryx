import { AzureFunction, Context, HttpRequest } from "@azure/functions";
import { v4 as uuid } from "uuid";

const notIn = (obj: any, key: string) => !(key in obj);

const isValidCharacter = (character: { [key: string]: any }) => {
  if (!character) {
    return false;
  }
  const notInCharacter = notIn.bind(null, character);
  // test for top-level fields
  if (
    notInCharacter("appearance") ||
    notInCharacter("stats") ||
    notInCharacter("companyInfo") ||
    notInCharacter("startedAt") ||
    notInCharacter("completedAt")
  ) {
    return false;
  }

  const notInAppearance = notIn.bind(null, character.appearance);

  if (
    notInAppearance("hair") ||
    notInAppearance("facialHair") ||
    notInAppearance("skin") ||
    notInAppearance("eyewear") ||
    notInAppearance("t-shirt") ||
    notInAppearance("shirt") ||
    notInAppearance("jacket") ||
    notInAppearance("hoodie") ||
    notInAppearance("bottom-clothes") ||
    notInAppearance("shoes")
  ) {
    return false;
  }

  return true;
};

const httpTrigger: AzureFunction = async function (context: Context, req: HttpRequest): Promise<void> {
  const character = req.body;

  if (!isValidCharacter(character)) {
    context.res = {
      status: 400,
      body: "Invalid character data",
    };
  } else {
    character.id = uuid();
    context.bindings.character = character;
    context.res = {
      // status: 200, /* Defaults to 200 */
      body: {
        success: true,
        public_url: `${process.env.SHORT_URL || "https://avtr.ms"}/${character.id}`,
      },
    };
  }
};

export default httpTrigger;
