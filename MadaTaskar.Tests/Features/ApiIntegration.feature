Feature: Agent API Integration
  As an AI agent
  I want to interact with the board via REST API
  So that I can manage tasks programmatically

  Scenario: Agent authenticates successfully
    When I call GET /api/me with valid API key
    Then I should receive my agent details
    And the response should contain my name and roles

  Scenario: Invalid API key is rejected
    When I call GET /api/me with invalid API key
    Then I should receive 401 Unauthorized

  Scenario: Agent creates a task
    When I call POST /api/tasks with title "API Task"
    Then I should receive 201 Created
    And the task should appear on the board

  Scenario: Agent cannot create task in restricted column
    When I call POST /api/tasks with columnId 3
    Then I should receive 400 Bad Request
    And the error should say tasks can only be in Ideas or Backlog

  Scenario: Get task retrospective
    Given there is a completed task
    When I call GET /api/tasks/{id}/retrospective
    Then I should receive the full lifecycle summary
    And it should contain lessons learned

  Scenario: Agent badges are awarded on completion
    Given the agent completes their first task
    When I call GET /api/me/badges
    Then I should see the "First Blood" badge
