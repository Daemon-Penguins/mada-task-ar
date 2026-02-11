Feature: Agent Management
  As an administrator
  I want to add, edit, and manage agents
  So that AI agents can participate on the board

  Scenario: Add a new agent with multiple roles
    Given I am logged in as admin "user"
    And I am on the admin panel Agents tab
    When I click "Add Agent"
    And I enter agent name "Skipper"
    And I select roles "admin" and "architect"
    And I click Create
    Then I should see "Skipper" in the agents list
    And the agent should have roles "admin,architect"

  Scenario: Edit agent roles
    Given I am logged in as admin "user"
    And I am on the admin panel Agents tab
    When I click edit roles for agent "Rico"
    And I add role "reviewer"
    And I click Save
    Then agent "Rico" should have the updated roles

  Scenario: Deactivate an agent
    Given I am logged in as admin "user"
    And there is an active agent "TestAgent"
    When I deactivate agent "TestAgent"
    Then agent "TestAgent" should show as inactive
