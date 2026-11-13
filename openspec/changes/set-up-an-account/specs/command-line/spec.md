# Spec Delta

## MODIFIED Requirements

### Requirement: Help is printed on request
`cader --help`, `-h`, `/h`, `-?` and `/?` SHALL print Cader's help to standard output and exit 0. The help SHALL describe Cader, say where to support it (`Buy me a sliver of a Claude Max plan: https://buymeacoffee.com/simonvane`), list the `here`, `setup-terminal` and `setup-account` commands with their descriptions, and list the help and version options. It SHALL NOT list the commands Cader runs for itself.

#### Scenario: Help
- **WHEN** the user runs cader --help
- **THEN** standard output holds the help with the here, setup-terminal and setup-account commands, nothing is started, and the exit code is 0

#### Scenario: Help says where to support Cader
- **WHEN** the user runs cader --help
- **THEN** the description includes Buy me a sliver of a Claude Max plan: https://buymeacoffee.com/simonvane

#### Scenario: Every help spelling
- **WHEN** the user runs cader with -h, /h, -? or /?
- **THEN** the same help is printed and the exit code is 0

#### Scenario: Plumbing stays out of the help
- **WHEN** the help is printed
- **THEN** it does not mention claim-tab

#### Scenario: Question mark on its own
- **WHEN** the user runs cader ?
- **THEN** it is a parse error with exit code 1

## ADDED Requirements

### Requirement: setup-account explains itself
A help option anywhere after `setup-account` SHALL print its usage, its name argument and its `--mode`, `--root`, `--yes` and `--remove` options with their descriptions, write nothing and exit 0.

#### Scenario: Help for setup-account
- **WHEN** the user runs cader setup-account personal --help
- **THEN** standard output holds the usage with --mode, --root, --yes and --remove, no folder or file is written, and the exit code is 0
