# Guidlines

This document gives some rules when it comes to committing to this repository. First of all any contributions are welcomed.

* Codebase, branches and commits need to be written in english
* Branches and commits need to be written in lowercase

## Branching

When creating a new feature please branch from the current `develop` branch.

Stick to following naming conventions:
* New Feature: feature/[Short name that describes feature]-[Your name or nickname] | example: `feature/login-matzy`
* Bugfix: fix/[Short name for bugfix]-[Your name or nickname] | example: `fix/wrong-websocket-address-matzy`

## Merging

When a feature is successfully written, create a merge-request on github.
Don't forget to add a reviewer, link the related issue (workload) and enable `delete branch` option.
Respectively reviewers can be `@matzy-byte` or anyone else that is responsible in that area.
When all reviewers accept, you can merge the branch into the newest develop branch.

For merging the `develop` branch into a `release` a bigger code-review is being required.
