import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProdutosPageComponent } from './produtos-page.component';

describe('ProdutosPageComponent', () => {
  let component: ProdutosPageComponent;
  let fixture: ComponentFixture<ProdutosPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProdutosPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProdutosPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
