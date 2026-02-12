Feature: Task Lifecycle Pipeline
  As a team member
  I want tasks to follow the defined pipeline
  So that every task goes through proper research, review, and acceptance

  Background:
    Given the test agent "TestBot" is registered

  Scenario: New task starts in Research phase
    Given I create a task "Pipeline Test" via API
    Then the task should be in "Research" phase
    And the task should be in "Backlog" column

  Scenario: Full task lifecycle via API
    Given I create a task "Full Lifecycle" via API
    When the agent adds research reference "https://example.com" with title "Reference"
    And the agent advances the task to "Brainstorm" phase
    And the agent adds a proposal "Let's use approach A"
    And the agent advances the task to "Triage" phase
    And the agent advances the task to "AuthorReview" phase
    And the author sets ReadyToWork to true
    And the agent advances the task to "ReadyToWork" phase
    And the agent assigns the task to themselves
    And the agent advances the task to "InProgress" phase
    And the agent advances the task to "Acceptance" phase
    And the agent approves the task
    Then the task should be in "Completed" phase
    And the task should be in "Done" column

  Scenario: Invalid phase transition returns error
    Given I create a task "Invalid Transition" via API
    When the agent tries to advance directly to "InProgress" phase
    Then the API should return error 400
    And the error should explain allowed transitions

  Scenario: Acceptance criteria must be met
    Given I create a task "Criteria Test" via API with acceptance criteria
    When I try to auto-accept the task
    Then it should fail because not all criteria are met
