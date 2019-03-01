# Styleguide UI Testing

This tool allows you to check if any of your partials differs from its previous version.

#### How does it work?
1. It fetches list of partial names from given styleguid url
2. It goes through all partials and make a screenshot 
3. It compares screenshot with one with the same name, stored in `accepted` folder (more info about location of this folder below)
4. If screenshots differ it makes diff image and stores this image in `diff` folder
5. If any of partial differes whole process finishes with error

## Storage 

### Local storage

This option is useful when you want to run this tool locally.

By default this tool will look for `accepted` and write to `diff` folders locally, in root directory. Just put screenshots in `accepted` folder and search for diff images in `diff` folder. 


### Azure storage

This option is useful when you want to run this tool on build agent which most probably has problems with storage as it is provisioned on each build.

In order to use Azure Storage you need to two environment variables with self explanatory values:

```
AZURE_STORAGE_CONNECTION_STRING=
AZURE_STORAGE_CONTAINER_NAME=
```

## Configuration

All configuration is done through environment variables

### STYLEGUIDE_URL (required)
Base url to styleguide, like `https://example.com/styleguide`

### EXCLUDE_PARTIALS
For some reason you want to exclude some partials from being compared (for example ones with animated elements don't will always be different, because of animation mutation).

In this case you may pass semicolon (`;`) separated list of partials:

`EXCLUDE_PARTIALS=PartialOne;PartialTwo`

### Final notes

Images need to be in png format



