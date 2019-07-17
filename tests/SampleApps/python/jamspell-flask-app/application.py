from flask import Flask
import jamspell

app = Flask(__name__)

corrector = jamspell.TSpellCorrector()
corrector.LoadLangModel('en.bin')

inputString = '[I am the begt spell cherken!]'
spellCheckedString = corrector.FixFragment(inputString )
# u'I am the best spell checker!'

@app.route("/")

def hello():
    return print  inputString  + " is spellchecked and corrected as " + spellCheckedString

