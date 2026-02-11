Feature: User Authentication
  As a team member
  I want to log in to the board
  So that I can manage tasks and agents

  Scenario: Successful login with valid credentials
    Given I am on the login page
    When I enter username "user" and password "password"
    And I click the login button
    Then I should be redirected to the board
    And I should see "Mada-TASK-ar" in the header

  Scenario: Failed login with wrong password
    Given I am on the login page
    When I enter username "user" and password "wrongpassword"
    And I click the login button
    Then I should see an error message
    And I should remain on the login page

  Scenario: Logout
    Given I am logged in as "user"
    When I click the logout button
    Then I should be redirected to the login page
