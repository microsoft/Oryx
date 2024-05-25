# Hugo conference

Hugo conference is a theme for conferences/events based on the original [conf-boilerplate theme](https://github.com/braziljs/conf-boilerplate/) by [BrazilJS Foundation](http://braziljs.org/) and his many contributors.

## Building my conference site from scratch

1. Install [Hugo](https://gohugo.io)
2. Create a new site by running:

        hugo new site my-conf
        cd my-conf
        git clone https://github.com/jweslley/hugo-conference themes/hugo-conference
        rm -f config.toml
        cp themes/hugo-conference/exampleSite/config.yml .

3. It's done. Just start Hugo server to see it live!

        hugo server --watch


## Customizing the site

All the site information can be found in the `config.yml` file. Just edit it to make changes.
By default, the site have the following sections:

- About - to describe what's the main goal of your event.
- Location - to show where it's going to happen through Google Maps.
- Speakers - to list information about speakers.
- Schedule - to show the agenda.
- Sponsors - to show the brand of your sponsors.
- Partners - to show the brand of your partners.

Ps: It's important to change the `baseurl` property from `config.yml` file in order to reflect your settings.

### Google Maps

Google now requires a Google Maps JavaScript API Key to show maps. You can obtain your key [here](https://developers.google.com/maps/documentation/javascript/get-api-key). Then set your API key in the `GoogleMapsKey` param in the `config.yml` file.

## License

MIT, see [LICENSE](https://github.com/jweslley/hugo-conference/blob/master/LICENSE).

