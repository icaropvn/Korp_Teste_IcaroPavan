import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NotasPageComponent } from './notas-page.component';

describe('NotasPageComponent', () => {
  let component: NotasPageComponent;
  let fixture: ComponentFixture<NotasPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NotasPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NotasPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
