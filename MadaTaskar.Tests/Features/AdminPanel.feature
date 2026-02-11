Feature: Admin Panel
  As an administrator
  I want to manage agents and users
  So that I can control who has access to the board

  Scenario: Admin can access admin panel
    Given I am logged in as admin "user"
    When I navigate to the admin panel
    Then I should see the Agents tab
    And I should see the Users tab
    And I should see the Activity Log tab

  Scenario: View registered agents
    Given I am logged in as admin "user"
    And I am on the admin panel
    When I click the Agents tab
    Then I should see agent "Rico" in the list
    And I should see their roles and status
