Feature: User Management
  As an administrator
  I want to manage user accounts
  So that team members can access the board

  Scenario: Add a new user
    Given I am logged in as admin "user"
    And I am on the admin panel Users tab
    When I click "Add User"
    And I fill in username "newuser", password "newpass", display name "New User"
    And I click Create
    Then I should see "newuser" in the users list

  Scenario: Delete a user
    Given I am logged in as admin "user"
    And there is a user "tempuser"
    When I delete user "tempuser"
    Then "tempuser" should not appear in the users list

  Scenario: Cannot delete yourself
    Given I am logged in as admin "user"
    Then I should not see a delete button next to my own account
