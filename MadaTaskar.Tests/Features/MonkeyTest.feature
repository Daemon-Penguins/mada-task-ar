Feature: Monkey Testing
  As a QA engineer
  I want to randomly click every available button
  So that I can find unexpected crashes and errors

  Scenario: Click all buttons on the board page
    Given I am logged in
    And I am on the board page
    When I click every non-disabled button on the page
    Then no JavaScript errors should appear in the console
    And the page should not crash

  Scenario: Click all buttons in admin panel
    Given I am logged in as admin
    And I am on the admin panel
    When I click every non-disabled button including in dialogs
    Then no JavaScript errors should appear in the console

  Scenario: Click all buttons in task dialog
    Given I am logged in
    And I open a task dialog
    When I click every non-disabled interactive element
    And I check all expandable panels
    Then no JavaScript errors should appear in the console

  Scenario: Rapid actions stress test
    Given I am logged in
    When I rapidly create 10 tasks
    And I rapidly move them between columns
    Then the board should remain stable
    And no console errors should appear
