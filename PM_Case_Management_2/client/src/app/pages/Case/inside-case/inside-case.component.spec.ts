import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InsideCaseComponent } from './inside-case.component';

describe('InsideCaseComponent', () => {
  let component: InsideCaseComponent;
  let fixture: ComponentFixture<InsideCaseComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ InsideCaseComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(InsideCaseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
