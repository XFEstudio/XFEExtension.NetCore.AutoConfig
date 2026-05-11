# Publishing Packages

This repository now includes two GitHub Actions workflows:

- `Validate`: runs on `push` and `pull_request` to ensure the solution restores and builds cleanly.
- `Publish Packages`: runs when you push a tag that starts with `v`, for example `v3.0.9`, or when you trigger it manually from the Actions page.

## Required secrets

Add this repository secret before publishing to NuGet.org:

- `NUGET_API_KEY`: API key from NuGet.org with push permission for `XFEExtension.NetCore.AutoConfig`

No extra secret is required for GitHub Packages. The workflow uses the built-in `GITHUB_TOKEN` and requests `packages: write`.

The same built-in `GITHUB_TOKEN` is also used to create the GitHub Release and upload package assets.

## Recommended release flow

1. Update the project and verify CI is green.
2. Push a version tag such as `v3.0.9`.
3. GitHub Actions will automatically do all of the following:
   - build and pack the project
   - upload the generated `.nupkg` as a workflow artifact
   - publish the package to NuGet.org
   - publish the package to GitHub Packages at `https://nuget.pkg.github.com/XFEstudio/index.json`
   - create a GitHub Release for that tag
   - attach the generated `.nupkg` files to the GitHub Release

## Tag-triggered release behavior

When you push a new tag like `v3.0.9`, that single action is the release trigger for the entire pipeline. You do not need to manually start the workflow afterward.

The release workflow is tied to `push.tags: v*`, so each new version tag automatically runs the full publishing flow.

## Manual publish

If you need to republish packages from the GitHub Actions UI:

1. Open `Publish Packages`.
2. Click `Run workflow`.
3. Fill in the `version` input.
4. Optionally set `skip_nuget_org` to `true` when you only want GitHub Packages.

Manual runs publish packages, but GitHub Release creation is only performed for real tag pushes so the release stays aligned with the repository tag history.
