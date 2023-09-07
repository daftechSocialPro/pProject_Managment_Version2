import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddInsideCaseComponent } from './add-inside-case.component';

describe('AddInsideCaseComponent', () => {
  let component: AddInsideCaseComponent;
  let fixture: ComponentFixture<AddInsideCaseComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AddInsideCaseComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddInsideCaseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
